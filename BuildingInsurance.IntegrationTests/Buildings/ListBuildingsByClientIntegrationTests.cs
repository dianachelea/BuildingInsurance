using BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.BuildingsRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Buildings
{
    public sealed class ListBuildingsByClientIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListBuildingsByClientHandler _handler;

        private Guid _clientIdWithBuildings;
        private Guid _clientIdWithoutBuildings;
        private Guid _cityId;

        public ListBuildingsByClientIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var buildingRepo = new BuildingRepository(_db);
            _handler = new ListBuildingsByClientHandler(buildingRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);

            _cityId = Guid.NewGuid();
            var city = new Domain.Entities.Geography.City(_cityId, "Cluj-Napoca", county.Id);
            _db.Cities.Add(city);

            _clientIdWithBuildings = Guid.NewGuid();
            var clientWithBuildings = new Domain.Entities.Clients.Client(
                id: _clientIdWithBuildings,
                type: ClientType.Individual,
                fullName: "Client With Buildings",
                contactInfo: new ContactInfo("with@x.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null);

            _clientIdWithoutBuildings = Guid.NewGuid();
            var clientWithoutBuildings = new Domain.Entities.Clients.Client(
                id: _clientIdWithoutBuildings,
                type: ClientType.Individual,
                fullName: "Client Without Buildings",
                contactInfo: new ContactInfo("without@x.com", "0700000000", null),
                personalIdentificationNumber: "2222222222222",
                companyRegistrationNumber: null);

            _db.Clients.AddRange(clientWithBuildings, clientWithoutBuildings);

            var b1 = new Building(
                id: Guid.NewGuid(),
                clientId: _clientIdWithBuildings,
                address: new Address("A", "1"),
                cityId: _cityId,
                constructionYear: 1990,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 50m,
                insuredValue: 50_000m,
                riskIndicators: RiskIndicators.None);

            var b2 = new Building(
                id: Guid.NewGuid(),
                clientId: _clientIdWithBuildings,
                address: new Address("B", "2"),
                cityId: _cityId,
                constructionYear: 1991,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 60m,
                insuredValue: 60_000m,
                riskIndicators: RiskIndicators.None);

            var b3 = new Building(
                id: Guid.NewGuid(),
                clientId: _clientIdWithBuildings,
                address: new Address("C", "3"),
                cityId: _cityId,
                constructionYear: 1992,
                type: BuildingType.Industrial,
                numberOfFloors: 3,
                surfaceArea: 70m,
                insuredValue: 70_000m,
                riskIndicators: RiskIndicators.None);

            var b4 = new Building(
                id: Guid.NewGuid(),
                clientId: _clientIdWithBuildings,
                address: new Address("D", "4"),
                cityId: _cityId,
                constructionYear: 1993,
                type: BuildingType.Industrial,
                numberOfFloors: 4,
                surfaceArea: 80m,
                insuredValue: 80_000m,
                riskIndicators: RiskIndicators.None);

            var b5 = new Building(
                id: Guid.NewGuid(),
                clientId: _clientIdWithBuildings,
                address: new Address("E", "5"),
                cityId: _cityId,
                constructionYear: 1994,
                type: BuildingType.Residential,
                numberOfFloors: 5,
                surfaceArea: 90m,
                insuredValue: 90_000m,
                riskIndicators: RiskIndicators.None);

            _db.Buildings.AddRange(b1, b2, b3, b4, b5);

            _db.SaveChanges();
        }

        [Fact]
        public async Task ListBuildingsByClient_Page2_PageSize2_ShouldReturnPagedItems()
        {
            var query = new ListBuildingsByClientQuery{ ClientId = _clientIdWithBuildings, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.All(result.Value.Items, x => Assert.Equal(_clientIdWithBuildings, x.ClientId));

            Assert.All(result.Value.Items, x => Assert.Contains(x.Street, new[] { "A", "B", "C", "D", "E" }));
        }

        [Fact]
        public async Task ListBuildingsByClient_PageGreaterThanTotalPages_ShouldReturnEmptyItems()
        {
            var query = new ListBuildingsByClientQuery { ClientId = _clientIdWithBuildings, Page = 10, PageSize = 2};

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(5, result.Value.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
        }

        [Fact]
        public async Task ListBuildingsByClient_WhenClientHasNoBuildings_ShouldReturnEmpty()
        {
            var query = new ListBuildingsByClientQuery{ ClientId = _clientIdWithoutBuildings, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}