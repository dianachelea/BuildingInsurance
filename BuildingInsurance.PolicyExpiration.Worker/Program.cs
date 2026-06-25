using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies;
using BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Services;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Infrastructure.CachingServices;
using BuildingInsurance.Infrastructure.HostedServices;
using BuildingInsurance.Infrastructure.Jobs;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.BuildingsRepository;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using BuildingInsurance.Infrastructure.Repositories.ManagementRepository;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyReportsRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyRepository;
using BuildingInsurance.Infrastructure.Repositories.ReportsRepository;
using BuildingInsurance.Infrastructure.Time;
using BuildingInsurance.Policy.Worker.HostedServices;
using BuildingInsurance.PolicyExpiration.Worker.HostedServices;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddDbContext<BuildingInsuranceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BuildingInsuranceDb")));

builder.Services.AddSingleton<GeographyCache>();
builder.Services.AddSingleton<IGeographyCachingService>(sp => sp.GetRequiredService<GeographyCache>());
builder.Services.AddSingleton<CurrencyCache>();
builder.Services.AddSingleton<ICurrencyCachingService>(sp => sp.GetRequiredService<CurrencyCache>());
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICountyRepository, CountyRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IBrokerRepository, BrokerRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IRiskFactorConfigurationRepository, RiskFactorConfigurationRepository>();
builder.Services.AddScoped<IFeeConfigurationRepository, FeeConfigurationRepository>();
builder.Services.AddScoped<IPolicyReportsRepository, PolicyReportsRepository>();
builder.Services.AddScoped<IReportJobsRepository, ReportJobsRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByCountryStrategy>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByCountyStrategy>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByCityStrategy>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByBrokerStrategy>();
builder.Services.AddScoped<IPolicyReportStrategySelector, PolicyReportStrategySelector>();
builder.Services.AddScoped<IPolicyExpirationService, PolicyExpirationRunner>();
builder.Services.AddScoped<IPolicyReportFactsMaterializer, PolicyReportFactsMaterializer>();
builder.Services.AddScoped<IReportJobsRunner, ReportJobsRunner>();
builder.Services.AddScoped<IReportJobProcessor, ReportJobProcessor>();
builder.Services.AddHostedService<PolicyExpirationHostedService>();
builder.Services.AddHostedService<PolicyReportFactsHostedService>();
builder.Services.AddHostedService<ReportJobsHostedService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var geo = scope.ServiceProvider.GetRequiredService<IGeographyCachingService>();
    await geo.LoadAsync(CancellationToken.None);

    var currency = scope.ServiceProvider.GetRequiredService<ICurrencyCachingService>();
    await currency.LoadAsync(CancellationToken.None);
}

await host.RunAsync();