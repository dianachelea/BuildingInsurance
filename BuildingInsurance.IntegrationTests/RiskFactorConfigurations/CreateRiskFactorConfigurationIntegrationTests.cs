using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.CreateRiskFactorConfiguration;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Mapping;
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
    public sealed class CreateRiskFactorConfigurationIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;

        private readonly IRiskFactorTargetVerifier _targetValidator;
        private readonly CreateRiskFactorConfigurationCommandHandler _handler;

        public CreateRiskFactorConfigurationIntegrationTests()
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

            _targetValidator = new RiskFactorTargetVerifier(_uow);

            _handler = new CreateRiskFactorConfigurationCommandHandler(
                _uow,
                _targetValidator,
                NullLogger<CreateRiskFactorConfigurationCommandHandler>.Instance
            );

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);

            var city = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);
            _db.Cities.Add(city);

            _db.SaveChanges();
        }

        [Fact]
        public async Task CreateRiskFactorConfiguration_GeographicTarget_ValidInput_ShouldPersist()
        {
            var city = await _db.Cities.AsNoTracking().FirstAsync();

            var command = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = city.Id,
                BuildingType = null,
                AdjustmentPercentage = 0.10m,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);

            var persisted = await _db.RiskFactorConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == result.Value.Id);

            Assert.NotNull(persisted);
            Assert.Equal(command.Level.MapToDomainRiskFactorLevel(), persisted!.Level);
            Assert.Equal(command.ReferenceId, persisted.ReferenceId);
            Assert.Null(persisted.BuildingType);
            Assert.Equal(command.AdjustmentPercentage, persisted.AdjustmentPercentage);
            Assert.Equal(command.IsActive, persisted.IsActive);
        }

        [Fact]
        public async Task CreateRiskFactorConfiguration_BuildingTypeTarget_ValidInput_ShouldPersist()
        {
            var command = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.BuildingType,
                ReferenceId = null,
                BuildingType = BuildingTypeContract.Residential,
                AdjustmentPercentage = -0.15m,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);

            var persisted = await _db.RiskFactorConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == result.Value.Id);

            Assert.NotNull(persisted);
            Assert.Equal(RiskFactorLevel.BuildingType, persisted!.Level);
            Assert.Null(persisted.ReferenceId);
            Assert.Equal(BuildingType.Residential, persisted.BuildingType);
            Assert.Equal(command.AdjustmentPercentage, persisted.AdjustmentPercentage);
            Assert.True(persisted.IsActive);
        }

        [Fact]
        public async Task CreateRiskFactorConfiguration_TargetAlreadyExists_ShouldReturnConflict_AndNotPersistDuplicate()
        {
            var city = await _db.Cities.AsNoTracking().FirstAsync();

            var existing = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: city.Id,
                buildingType: null,
                adjustmentPercentage: 0.20m,
                isActive: true);

            _db.RiskFactorConfigurations.Add(existing);
            await _db.SaveChangesAsync();

            var command = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = city.Id,
                BuildingType = null,
                AdjustmentPercentage = 0.10m,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Risk factor configuration already exists for the provided target.", result.Error);

            var count = await _db.RiskFactorConfigurations
                .AsNoTracking()
                .CountAsync(x => x.Level == RiskFactorLevel.City && x.ReferenceId == city.Id && x.BuildingType == null);

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CreateRiskFactorConfiguration_CityNotFound_ShouldReturnNotFound_AndNotPersist()
        {
            var command = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
                BuildingType = null,
                AdjustmentPercentage = 0.10m,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("City not found.", result.Error);

            var exists = await _db.RiskFactorConfigurations.AsNoTracking().AnyAsync(x => x.Level == RiskFactorLevel.City && x.ReferenceId == command.ReferenceId);

            Assert.False(exists);
        }

        [Fact]
        public async Task CreateRiskFactorConfiguration_WhenCommitFails_ShouldRollback()
        {
            var city = await _db.Cities.AsNoTracking().FirstAsync();

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

            var failingValidator = new RiskFactorTargetVerifier(failingUow);

            var failingHandler = new CreateRiskFactorConfigurationCommandHandler(failingUow, failingValidator, NullLogger<CreateRiskFactorConfigurationCommandHandler>.Instance);

            var command = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = city.Id,
                BuildingType = null,
                AdjustmentPercentage = 0.12m,
                IsActive = true
            };

            var result = await failingHandler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during RiskFactorConfiguration creation.", result.Error);

            var exists = await failingContext.RiskFactorConfigurations.AsNoTracking()
                .AnyAsync(x => x.Level == RiskFactorLevel.City
                            && x.ReferenceId == city.Id
                            && x.AdjustmentPercentage == 0.12m);

            Assert.False(exists, "RiskFactorConfiguration should not exist in DB because transaction should have rolled back.");
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}