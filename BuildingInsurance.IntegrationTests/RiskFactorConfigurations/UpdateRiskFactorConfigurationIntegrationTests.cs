using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.UpdateRiskFactorConfiguration;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Result;
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

namespace BuildingInsurance.IntegrationTests.RiskFactorConfigurations
{
    public sealed class UpdateRiskFactorConfigurationIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly UpdateRiskFactorConfigurationCommandHandler _handler;

        private readonly Guid _toUpdateId = Guid.NewGuid();
        private readonly Guid _existingTargetId = Guid.NewGuid();
        private readonly Guid _otherCityId = Guid.NewGuid();

        public UpdateRiskFactorConfigurationIntegrationTests()
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

            _uow = new UnitOfWork(_db, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskFactorConfigRepo, feeConfigRepo);

            _handler = new UpdateRiskFactorConfigurationCommandHandler(_uow, new RiskFactorTargetVerifier(_uow), NullLogger<UpdateRiskFactorConfigurationCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);

            var city1 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);
            var city2 = new Domain.Entities.Geography.City(_otherCityId, "Gherla", county.Id);
            _db.Cities.AddRange(city1, city2);

            var toUpdate = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: city1.Id,
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true
            );
            typeof(Domain.Entities.Metadata.RiskFactorConfiguration)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
                .SetValue(toUpdate, _toUpdateId);

            var existingTarget = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.County,
                referenceId: county.Id,
                buildingType: null,
                adjustmentPercentage: 0.05m,
                isActive: true
            );
            typeof(Domain.Entities.Metadata.RiskFactorConfiguration)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
                .SetValue(existingTarget, _existingTargetId);

            var other = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.BuildingType,
                referenceId: null,
                buildingType: BuildingType.Residential,
                adjustmentPercentage: -0.02m,
                isActive: true
            );

            _db.RiskFactorConfigurations.AddRange(toUpdate, existingTarget, other);
            _db.SaveChanges();
        }

        [Fact]
        public async Task UpdateRiskFactorConfiguration_WhenNotFound_ShouldReturnNotFound()
        {
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.City,
                ReferenceId = _otherCityId,
                BuildingType = null,
                AdjustmentPercentage = 0.11m,
                IsActive = true
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateRiskFactorConfiguration_WhenTargetExists_ShouldReturnConflict_AndNotPersist()
        {
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = _toUpdateId,
                Level = RiskFactorLevelContract.County,
                ReferenceId = (await _db.Counties.AsNoTracking().Select(c => c.Id).FirstAsync()),
                BuildingType = null,
                AdjustmentPercentage = 0.20m,
                IsActive = true
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Risk factor configuration already exists for the provided target.", result.Error);

            var persisted = await _db.RiskFactorConfigurations.AsNoTracking().FirstAsync(r => r.Id == _toUpdateId);
            Assert.Equal(RiskFactorLevel.City, persisted.Level);
            Assert.NotEqual(0.20m, persisted.AdjustmentPercentage);
        }

        [Fact]
        public async Task UpdateRiskFactorConfiguration_ValidUpdate_ShouldPersistChanges()
        {
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = _toUpdateId,
                Level = RiskFactorLevelContract.City,
                ReferenceId = (await _db.Cities.Select(c => c.Id).FirstAsync()),
                BuildingType = null,
                AdjustmentPercentage = 0.25m,
                IsActive = false
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var dto = result.Value!;
            Assert.Equal(_toUpdateId, dto.Id);
            Assert.Equal(0.25m, dto.AdjustmentPercentage);
            Assert.False(dto.IsActive);

            var persisted = await _db.RiskFactorConfigurations.AsNoTracking().FirstAsync(r => r.Id == _toUpdateId);
            Assert.Equal(0.25m, persisted.AdjustmentPercentage);
            Assert.False(persisted.IsActive);
        }

        [Fact]
        public async Task UpdateRiskFactorConfiguration_WhenCommitFails_ShouldRollback()
        {
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = _toUpdateId,
                Level = RiskFactorLevelContract.City,
                ReferenceId = (await _db.Cities.Select(c => c.Id).FirstAsync()),
                BuildingType = null,
                AdjustmentPercentage = 0.33m,
                IsActive = true
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

            var failingUow = new UnitOfWork(
                failingContext,
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

            var failingHandler = new UpdateRiskFactorConfigurationCommandHandler(
                failingUow,
                new RiskFactorTargetVerifier(failingUow),
                NullLogger<UpdateRiskFactorConfigurationCommandHandler>.Instance
            );

            var result = await failingHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error while updating risk factor configuration.", result.Error);

            var exists = await failingContext.RiskFactorConfigurations.AsNoTracking()
                .AnyAsync(r => r.Id == _toUpdateId && r.AdjustmentPercentage == 0.33m);

            Assert.False(exists, "Update should not be present in DB because commit failed and transaction rolled back.");
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}