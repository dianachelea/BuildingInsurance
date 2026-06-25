using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Infrastructure.CachingServices;
using BuildingInsurance.Infrastructure.HostedServices;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using System.Reflection;

namespace BuildingInsurance.IntegrationTests.Endpoints
{
    public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTests");

            builder.UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.IntegrationTests.json", optional: false, reloadOnChange: false)
                      .AddEnvironmentVariables();
            });

            builder.ConfigureServices((context, services) =>
            {
                var optionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<BuildingInsuranceDbContext>));
                if (optionsDescriptor != null)
                    services.Remove(optionsDescriptor);

                var connectionString = context.Configuration.GetConnectionString("BuildingInsuranceDb_Test");
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Connection string 'BuildingInsuranceDb_Test' not found.");

                services.AddDbContext<BuildingInsuranceDbContext>(opt =>
                    opt.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly("BuildingInsurance.Infrastructure")));

                services.RemoveAll<IGeographyCachingService>();
                services.RemoveAll<GeographyCache>();

                services.AddSingleton<IGeographyCachingService, FakeGeographyCachingService>();

                services.RemoveAll<ICurrencyCachingService>();
                services.RemoveAll<CurrencyCache>();
                services.AddSingleton<ICurrencyCachingService, FakeCurrencyCachingService>();
            });
        }

        private sealed class FakeCurrencyCachingService : ICurrencyCachingService
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public FakeCurrencyCachingService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

            public Task LoadAsync(CancellationToken ct) => Task.CompletedTask;

            public bool TryGetCode(Guid currencyId, out string code)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                var currency = db.Currencies.AsNoTracking().FirstOrDefault(c => c.Id == currencyId);
                if (currency is null) { code = ""; return false; }

                code = currency.Code.ToString();
                return true;
            }

            public bool TryGetId(string code, out Guid id)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                var currency = db.Currencies.AsNoTracking().FirstOrDefault(c => c.Code.ToString() == code);
                if (currency is null) { id = Guid.Empty; return false; }

                id = currency.Id;
                return true;
            }
        }

        private sealed class FakeGeographyCachingService : IGeographyCachingService
        {
            private readonly IServiceScopeFactory _scopeFactory;
            public FakeGeographyCachingService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

            public Task LoadAsync(CancellationToken ct)
            {
                return Task.CompletedTask;
            }

            public bool TryGet(Guid cityId, out string city, out string county, out string country)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                var cityEntity = db.Cities.AsNoTracking().FirstOrDefault(c => c.Id == cityId);
                if (cityEntity is null) { city = county = country = ""; return false; }

                var countyEntity = db.Counties.AsNoTracking().FirstOrDefault(cn => cn.Id == cityEntity.CountyId);
                if (countyEntity is null) { city = county = country = ""; return false; }

                var countryEntity = db.Countries.AsNoTracking().FirstOrDefault(ct => ct.Id == countyEntity.CountryId);
                if (countryEntity is null) { city = county = country = ""; return false; }

                city = cityEntity.Name;
                county = countyEntity.Name;
                country = countryEntity.Name;
                return true;
            }
        }
    }
}