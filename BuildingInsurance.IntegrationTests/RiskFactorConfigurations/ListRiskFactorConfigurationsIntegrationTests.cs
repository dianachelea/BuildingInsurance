using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.RiskFactorConfigurations
{
    public sealed class ListRiskFactorConfigurationsIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListRiskFactorConfigurationsHandler _handler;

        public ListRiskFactorConfigurationsIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var riskFactorConfigRepo = new RiskFactorConfigurationRepository(_db);
            _handler = new ListRiskFactorConfigurationsHandler(riskFactorConfigRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);

            var city1 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);
            var city2 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Gherla", county.Id);
            _db.Cities.AddRange(city1, city2);

            var r1 = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: city1.Id,
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var r2 = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.County,
                referenceId: county.Id,
                buildingType: null,
                adjustmentPercentage: 0.05m,
                isActive: true);

            var r3 = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.BuildingType,
                referenceId: null,
                buildingType: BuildingType.Residential,
                adjustmentPercentage: -0.02m,
                isActive: true);

            var r4 = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: city2.Id,
                buildingType: null,
                adjustmentPercentage: -0.05m,
                isActive: false);

            var r5 = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: country.Id,
                buildingType: null,
                adjustmentPercentage: 0.03m,
                isActive: true);

            _db.RiskFactorConfigurations.AddRange(r1, r2, r3, r4, r5);
            _db.SaveChanges();
        }

        [Fact]
        public async Task ListRiskFactorConfigurations_Page2_PageSize2_ShouldReturnPagedResults()
        {
            var query = new ListRiskFactorConfigurationsQuery{ Level = null, ReferenceId = null, IsActive = null, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;

            Assert.Equal(5, resp.TotalCount);
            Assert.Equal(3, resp.TotalPages);
            Assert.Equal(2, resp.Items.Count);
        }

        [Fact]
        public async Task ListRiskFactorConfigurations_FilterByLevel_BuildingType_ShouldReturnOnlyBuildingTypeConfigs()
        {
            var query = new ListRiskFactorConfigurationsQuery{ Level = RiskFactorLevelContract.BuildingType, ReferenceId = null, IsActive = null, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Equal(1, resp.TotalCount);
            Assert.Equal(1, resp.TotalPages);
            Assert.Single(resp.Items);

            Assert.All(resp.Items, item => Assert.Equal(RiskFactorLevel.BuildingType, item.Level));
        }

        [Fact]
        public async Task ListRiskFactorConfigurations_FilterByReferenceId_City_ShouldReturnOnlyCityConfigs()
        {
            var cityId = await _db.RiskFactorConfigurations
                .Where(r => r.Level == RiskFactorLevel.City && r.IsActive)
                .Select(r => r.ReferenceId!.Value)
                .FirstAsync();

            var query = new ListRiskFactorConfigurationsQuery{ Level = RiskFactorLevelContract.City, ReferenceId = cityId, IsActive = true, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Equal(1, resp.TotalCount);
            Assert.Equal(1, resp.TotalPages);
            Assert.Single(resp.Items);

            Assert.All(resp.Items, item =>
            {
                Assert.Equal(RiskFactorLevel.City, item.Level);
                Assert.Equal(cityId, item.ReferenceId);
            });
        }

        [Fact]
        public async Task ListRiskFactorConfigurations_FilterByIsActive_True_ShouldReturnOnlyActiveConfigs()
        {
            var query = new ListRiskFactorConfigurationsQuery{ Level = null, ReferenceId = null, IsActive = true, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Equal(4, resp.TotalCount);
            Assert.Equal(1, resp.TotalPages);
            Assert.Equal(4, resp.Items.Count);

            Assert.All(resp.Items, item => Assert.True(item.IsActive));
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}