using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.GetRiskFactorConfigurationById;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.BuildingsRepository;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using BuildingInsurance.Infrastructure.Repositories.ManagementRepository;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.RiskFactorConfigurations
{
    public sealed class GetRiskFactorConfigurationByIdIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly GetRiskFactorConfigurationByIdHandler _handler;

        private readonly Guid _geoConfigId = Guid.NewGuid();
        private readonly Guid _buildingTypeConfigId = Guid.NewGuid();

        public GetRiskFactorConfigurationByIdIntegrationTests()
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

            _handler = new GetRiskFactorConfigurationByIdHandler(_uow);

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

            var geoConfig = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: city.Id,
                buildingType: null,
                adjustmentPercentage: 0.20m,
                isActive: true
            );

            var buildingTypeConfig = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.BuildingType,
                referenceId: null,
                buildingType: BuildingType.Industrial,
                adjustmentPercentage: -0.08m,
                isActive: false
            );

            typeof(Domain.Entities.Metadata.RiskFactorConfiguration)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(geoConfig, _geoConfigId);

            typeof(Domain.Entities.Metadata.RiskFactorConfiguration)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(buildingTypeConfig, _buildingTypeConfigId);

            _db.RiskFactorConfigurations.AddRange(geoConfig, buildingTypeConfig);

            _db.SaveChanges();
        }

        [Fact]
        public async Task GetRiskFactorConfigurationById_WhenNotFound_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();
            var query = new GetRiskFactorConfigurationByIdQuery(missingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Risk factor configuration not found.", result.Error);
        }

        [Fact]
        public async Task GetRiskFactorConfigurationById_WhenExists_GeographicConfig_ShouldReturnDto()
        {
            var query = new GetRiskFactorConfigurationByIdQuery(_geoConfigId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var dto = result.Value!;
            Assert.Equal(_geoConfigId, dto.Id);
            Assert.Equal(RiskFactorLevel.City, dto.Level);
            Assert.NotNull(dto.ReferenceId);
            Assert.Null(dto.BuildingType);
            Assert.Equal(0.20m, dto.AdjustmentPercentage);
            Assert.True(dto.IsActive);
            var persisted = await _db.RiskFactorConfigurations.AsNoTracking().FirstAsync(r => r.Id == _geoConfigId);
            Assert.Equal(RiskFactorLevel.City, persisted.Level);
            Assert.Equal(dto.ReferenceId, persisted.ReferenceId);
            Assert.Equal(dto.AdjustmentPercentage, persisted.AdjustmentPercentage);
            Assert.Equal(dto.IsActive, persisted.IsActive);
        }

        [Fact]
        public async Task GetRiskFactorConfigurationById_WhenExists_BuildingTypeConfig_ShouldReturnDto()
        {
            var query = new GetRiskFactorConfigurationByIdQuery(_buildingTypeConfigId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var dto = result.Value!;
            Assert.Equal(_buildingTypeConfigId, dto.Id);
            Assert.Equal(RiskFactorLevel.BuildingType, dto.Level);
            Assert.Null(dto.ReferenceId);
            Assert.Equal(BuildingType.Industrial, dto.BuildingType);
            Assert.Equal(-0.08m, dto.AdjustmentPercentage);
            Assert.False(dto.IsActive);

            var persisted = await _db.RiskFactorConfigurations.AsNoTracking().FirstAsync(r => r.Id == _buildingTypeConfigId);
            Assert.Equal(RiskFactorLevel.BuildingType, persisted.Level);
            Assert.Equal(dto.BuildingType, persisted.BuildingType);
            Assert.Equal(dto.AdjustmentPercentage, persisted.AdjustmentPercentage);
            Assert.Equal(dto.IsActive, persisted.IsActive);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}