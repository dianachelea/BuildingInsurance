using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.UpdateFeeConfiguration;
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
    public sealed class UpdateFeeConfigurationIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly UpdateFeeConfigurationCommandHandler _handler;

        private Guid _feeToUpdateId;
        private Guid _overlapFeeId;

        public UpdateFeeConfigurationIntegrationTests()
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

            _handler = new UpdateFeeConfigurationCommandHandler(_uow, NullLogger<UpdateFeeConfigurationCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var fee1 = new FeeConfiguration(
                feeName: "Admin Fee 2027 - A",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2027, 06, 30, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var fee2 = new FeeConfiguration(
                feeName: "Admin Fee 2027 - B",
                feeType: FeeType.AdminFee,
                feePercentage: 0.11m,
                effectiveFrom: new DateTime(2027, 07, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            _db.FeeConfigurations.AddRange(fee1, fee2);
            _db.SaveChanges();

            _feeToUpdateId = fee1.Id;
            _overlapFeeId = fee2.Id;
        }

        [Fact]
        public async Task UpdateFeeConfiguration_WhenNotFound_ShouldReturnNotFound()
        {
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Updated",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.12m,
                EffectiveFrom = new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Fee configuration with ID {cmd.Id} not found.", result.Error);
        }

        [Fact]
        public async Task UpdateFeeConfiguration_WhenOverlapsOtherConfig_ShouldReturnConflict_AndNotPersist()
        {
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = _feeToUpdateId,
                Name = "Admin Fee 2027 - A (Updated)",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.20m,
                EffectiveFrom = new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another fee configuration exists with overlapping period for the same type and risk indicators.", result.Error);

            var persisted = await _db.FeeConfigurations.AsNoTracking().FirstAsync(f => f.Id == _feeToUpdateId);
            Assert.Equal("Admin Fee 2027 - A", persisted.Name);
            Assert.Equal(0.10m, persisted.FeePercentage);
        }

        [Fact]
        public async Task UpdateFeeConfiguration_ValidInput_ShouldPersist()
        {
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = _feeToUpdateId,
                Name = "Admin Fee 2027 - A (Updated)",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.12m,
                EffectiveFrom = new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2027, 06, 30, 0, 0, 0, DateTimeKind.Utc),
                IsActive = false,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var persisted = await _db.FeeConfigurations.AsNoTracking().FirstAsync(f => f.Id == _feeToUpdateId);
            Assert.Equal("Admin Fee 2027 - A (Updated)", persisted.Name);
            Assert.Equal(0.12m, persisted.FeePercentage);
            Assert.False(persisted.IsActive);

            Assert.Equal(new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc), persisted.EffectiveFrom);
            Assert.Equal(new DateTime(2027, 06, 30, 0, 0, 0, DateTimeKind.Utc), persisted.EffectiveTo);
        }

        [Fact]
        public async Task UpdateFeeConfiguration_WhenCommitFails_ShouldRollback()
        {
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = _feeToUpdateId,
                Name = "Should Not Persist",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.30m,
                EffectiveFrom = new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                EffectiveTo = new DateTime(2027, 06, 30, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

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

            var failingHandler = new UpdateFeeConfigurationCommandHandler(
                failingUow,
                NullLogger<UpdateFeeConfigurationCommandHandler>.Instance
            );

            var result = await failingHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error while updating fee configuration.", result.Error);

            var updatedExists = await failingContext.FeeConfigurations.AsNoTracking()
                .AnyAsync(f => f.Id == _feeToUpdateId && f.Name == "Should Not Persist" && f.FeePercentage == 0.30m);

            Assert.False(updatedExists);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}