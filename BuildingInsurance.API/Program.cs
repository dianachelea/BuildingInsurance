using BuildingInsurance.API.Middlewares;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies;
using BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Brokers.Policies.Services;
using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Selection;
using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Behaviors;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Infrastructure.CachingServices;
using BuildingInsurance.Infrastructure.HostedServices;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Persistence.Seeding;
using BuildingInsurance.Infrastructure.Repositories.BuildingsRepository;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using BuildingInsurance.Infrastructure.Repositories.ManagementRepository;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyReportsRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyRepository;
using BuildingInsurance.Infrastructure.Repositories.ReportsRepository;
using BuildingInsurance.Infrastructure.Time;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: true)
        );
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BuildingInsuranceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BuildingInsuranceDb")));

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateClientCommand>();
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateClientCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddSingleton<GeographyCache>();
builder.Services.AddSingleton<IGeographyCachingService>(sp => sp.GetRequiredService<GeographyCache>());
builder.Services.AddSingleton<CurrencyCache>();
builder.Services.AddSingleton<ICurrencyCachingService>(sp => sp.GetRequiredService<CurrencyCache>());
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRiskFactorTargetVerifier, RiskFactorTargetVerifier>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICountyRepository, CountyRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IBrokerRepository, BrokerRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IPolicyReportsRepository, PolicyReportsRepository>();
builder.Services.AddScoped<IRiskFactorConfigurationRepository, RiskFactorConfigurationRepository>();
builder.Services.AddScoped<IFeeConfigurationRepository, FeeConfigurationRepository>();
builder.Services.AddScoped<IPolicyPricingService, PolicyPricingService>();
builder.Services.AddScoped<IPolicyPricingStrategy, DraftPolicyPricingStrategy>();
builder.Services.AddScoped<IPolicyPricingStrategy, SnapshotPolicyPricingStrategy>();
builder.Services.AddScoped<IPolicyPricingStrategySelector, PolicyPricingStrategySelector>();
builder.Services.AddScoped<IClientBuildingVerifier, ClientBuildingVerifier>();
builder.Services.AddScoped<ICurrencyVerifier, CurrencyVerifier>();
builder.Services.AddScoped<IBrokerVerifier, BrokerVerifier>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByCountryStrategy>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByCountyStrategy>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByCityStrategy>();
builder.Services.AddScoped<IPolicyReportStrategy, PoliciesByBrokerStrategy>();
builder.Services.AddScoped<IReportJobsRepository, ReportJobsRepository>();
builder.Services.AddScoped<IPolicyReportStrategySelector, PolicyReportStrategySelector>();

var app = builder.Build();
app.UseResultExceptionHandling();

if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

    await DataSeeder.SeedAsync(db);

    var geo = scope.ServiceProvider.GetRequiredService<IGeographyCachingService>();
    await geo.LoadAsync(CancellationToken.None);

    var currency = scope.ServiceProvider.GetRequiredService<ICurrencyCachingService>();
    await currency.LoadAsync(CancellationToken.None);
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();

public partial class Program { }