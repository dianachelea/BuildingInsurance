using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.CreateFeeConfiguration;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.BuildingsRepository;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using BuildingInsurance.Infrastructure.Repositories.ManagementRepository;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyRepository;
using BuildingInsurance.IntegrationTests.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BuildingInsurance.IntegrationTests.FeeConfigurations
{
    public sealed class CreateFeeConfigurationIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly CreateFeeConfigurationCommandHandler _handler;

        public CreateFeeConfigurationIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var clientRepo = new ClientRepository(_db);
            var buildingRepo = new BuildingRepository(_db);
            var cityRepo = new CityRepository(_db);
            var countyRepo = new CountyRepository(_db);
            var countryRepo = new CountryRepository(_db);
            var policyRepo = new PolicyRepository(_db);
            var brokerRepo = new BrokerRepository(_db);
            var currencyRepo = new CurrencyRepository(_db);
            var riskFactorConfigRepo = new RiskFactorConfigurationRepository(_db);
            var feeConfigRepo = new FeeConfigurationRepository(_db);

            _uow = new UnitOfWork(
                _db,
                clientRepo,
                buildingRepo,
                cityRepo,
                countyRepo,
                countryRepo,
                policyRepo,
                brokerRepo,
                currencyRepo,
                riskFactorConfigRepo,
                feeConfigRepo
            );

            _handler = new CreateFeeConfigurationCommandHandler(_uow, NullLogger<CreateFeeConfigurationCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var existing = new FeeConfiguration(
                feeName: "Existing Admin Fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None
            );

            _db.FeeConfigurations.Add(existing);
            _db.SaveChanges();
        }

        [Fact]
        public async Task CreateFeeConfiguration_ValidInput_ShouldPersist()
        {
            var command = new CreateFeeConfigurationCommand
            {
                Name = "New Broker Commission",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.15m,
                EffectiveFrom = new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);

            var persisted = await _db.FeeConfigurations.AsNoTracking().FirstOrDefaultAsync(f => f.Id == result.Value.Id);

            Assert.NotNull(persisted);
            Assert.Equal("New Broker Commission", persisted!.Name);
            Assert.Equal(FeeType.BrokerCommission, persisted.FeeType);
            Assert.Equal(0.15m, persisted.FeePercentage);
            Assert.True(persisted.IsActive);
            Assert.Equal(RiskIndicators.None, persisted.RiskIndicators);
            Assert.Equal(DateTimeKind.Utc, result.Value!.EffectiveFrom.Kind);
            Assert.Equal(DateTimeKind.Utc, result.Value!.EffectiveTo.Kind);
        }

        [Fact]
        public async Task CreateFeeConfiguration_WhenOverlappingPeriodExists_ShouldReturnConflict_AndNotPersist()
        {
            var command = new CreateFeeConfigurationCommand
            {
                Name = "Overlapping Admin Fee",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.12m,
                EffectiveFrom = new DateTime(2026, 06, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2026, 06, 30, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another fee configuration exists with overlapping period for the same type and risk indicators.", result.Error);

            var exists = await _db.FeeConfigurations.AsNoTracking()
                .AnyAsync(f => f.Name == "Overlapping Admin Fee");

            Assert.False(exists);
        }

        [Fact]
        public async Task CreateFeeConfiguration_RiskAdjustment_WithIndicators_ShouldPersist()
        {
            var command = new CreateFeeConfigurationCommand
            {
                Name = "Risk Adjustment - Flood",
                FeeType = FeeTypeContract.RiskAdjustment,
                FeePercentage = 0.05m,
                EffectiveFrom = new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var persisted = await _db.FeeConfigurations.AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == result.Value!.Id);

            Assert.NotNull(persisted);
            Assert.Equal(FeeType.RiskAdjustment, persisted!.FeeType);
            Assert.Equal(RiskIndicators.FloodZone, persisted.RiskIndicators);
        }

        [Fact]
        public async Task CreateFeeConfiguration_WhenCommitFails_ShouldRollback()
        {
            var optionsFail = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new SimulatedFailureInterceptor())
                .Options;

            using var failingContext = new BuildingInsuranceDbContext(optionsFail);
            await failingContext.Database.EnsureCreatedAsync();

            var clientRepo = new ClientRepository(failingContext);
            var buildingRepo = new BuildingRepository(failingContext);
            var cityRepo = new CityRepository(failingContext);
            var countyRepo = new CountyRepository(failingContext);
            var countryRepo = new CountryRepository(failingContext);
            var policyRepo = new PolicyRepository(failingContext);
            var brokerRepo = new BrokerRepository(failingContext);
            var currencyRepo = new CurrencyRepository(failingContext);
            var riskFactorConfigRepo = new RiskFactorConfigurationRepository(failingContext);
            var feeConfigRepo = new FeeConfigurationRepository(failingContext);

            var failingUow = new UnitOfWork(failingContext, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskFactorConfigRepo, feeConfigRepo);

            var failingHandler = new CreateFeeConfigurationCommandHandler(failingUow, NullLogger<CreateFeeConfigurationCommandHandler>.Instance);

            var command = new CreateFeeConfigurationCommand
            {
                Name = "Rollback Fee",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.20m,
                EffectiveFrom = new DateTime(2028, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await failingHandler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during fee configuration creation.", result.Error);

            var exists = await failingContext.FeeConfigurations.AsNoTracking()
                .AnyAsync(f => f.Name == "Rollback Fee");

            Assert.False(exists, "FeeConfiguration should not exist in DB because transaction should have rolled back.");
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}
