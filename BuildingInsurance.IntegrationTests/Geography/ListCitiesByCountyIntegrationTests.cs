using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Geography
{
    public sealed class ListCitiesByCountyIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListCitiesByCountyHandler _handler;

        private readonly Guid _countryId = Guid.NewGuid();
        private readonly Guid _countyId = Guid.NewGuid();

        public ListCitiesByCountyIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var cityRepo = new CityRepository(_db);
            _handler = new ListCitiesByCountyHandler(cityRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new BuildingInsurance.Domain.Entities.Geography.Country(_countryId, "Romania");
            _db.Countries.Add(country);

            var county = new BuildingInsurance.Domain.Entities.Geography.County(_countyId, "Cluj", _countryId);
            _db.Counties.Add(county);

            var c1 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", _countyId);
            var c2 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Apahida", _countyId);
            var c3 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Baciu", _countyId);
            var c4 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Dej", _countyId);
            var c5 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Eforie City", _countyId);

            _db.Cities.AddRange(c1, c2, c3, c4, c5);
            _db.SaveChanges();
        }

        [Fact]
        public async Task ListCities_Page2_PageSize2_ShouldReturnSortedItems()
        {
            var query = new ListCitiesByCountyQuery{ CountyId = _countyId, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("CLUJ-NAPOCA", result.Value.Items[0].Name);
            Assert.Equal("DEJ", result.Value.Items[1].Name);
        }
        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}