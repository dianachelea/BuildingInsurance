using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Geography
{
    public sealed class ListCountriesIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListCountriesHandler _handler;

        public ListCountriesIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var countryRepo = new CountryRepository(_db);
            _handler = new ListCountriesHandler(countryRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var romania = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            var austria = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Austria");
            var bulgaria = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Bulgaria");
            var denmark = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Denmark");
            var estonia = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Estonia");

            _db.Countries.AddRange(romania, austria, bulgaria, denmark, estonia);
            _db.SaveChanges();
        }

        [Fact]
        public async Task ListCountries_Page2_PageSize2_ShouldReturnSortedItems()
        {
            var query = new ListCountriesQuery { Page = 2, PageSize = 2};

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("DENMARK", result.Value.Items[0].Name);
            Assert.Equal("ESTONIA", result.Value.Items[1].Name);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}