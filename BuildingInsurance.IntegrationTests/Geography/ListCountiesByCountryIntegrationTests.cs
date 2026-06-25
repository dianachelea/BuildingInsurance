using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Geography
{
    public sealed class ListCountiesByCountryIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListCountiesByCountryHandler _handler;

        private readonly Guid _countryId = Guid.NewGuid();

        public ListCountiesByCountryIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var countyRepo = new CountyRepository(_db);
            _handler = new ListCountiesByCountryHandler(countyRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new Domain.Entities.Geography.Country(_countryId, "Romania");
            _db.Countries.Add(country);

            var cluj = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", _countryId);
            var alba = new Domain.Entities.Geography.County(Guid.NewGuid(), "Alba", _countryId);
            var bihor = new Domain.Entities.Geography.County(Guid.NewGuid(), "Bihor", _countryId);
            var dolj = new Domain.Entities.Geography.County(Guid.NewGuid(), "Dolj", _countryId);
            var eforie = new Domain.Entities.Geography.County(Guid.NewGuid(), "Eforie", _countryId);

            _db.Counties.AddRange(cluj, alba, bihor, dolj, eforie);
            _db.SaveChanges();
        }

        [Fact]
        public async Task ListCounties_Page2_PageSize2_ShouldReturnSortedItems()
        {
            var query = new ListCountiesByCountryQuery { CountryId = _countryId, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("CLUJ", result.Value.Items[0].Name);
            Assert.Equal("DOLJ", result.Value.Items[1].Name);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}