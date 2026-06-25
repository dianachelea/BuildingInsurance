using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Currencies
{
    public sealed class ListCurrenciesIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListCurrenciesHandler _handler;

        public ListCurrenciesIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var currencyRepo = new CurrencyRepository(_db);
            _handler = new ListCurrenciesHandler(currencyRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _db.Currencies.AddRange(
                new Domain.Entities.Metadata.Currency("EUR", "Euro", 1.0m, true),
                new Domain.Entities.Metadata.Currency("USD", "US Dollar", 4.50m, true),
                new Domain.Entities.Metadata.Currency("RON", "Romanian Leu", 1.0m, true)
            );

            _db.SaveChanges();
        }

        [Fact]
        public async Task ListCurrencies_Page2_PageSize2_ShouldReturnPagedResults()
        {
            var query = new ListCurrenciesQuery{ Name = null, IsActive = null, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;

            var expectedTotal = await _db.Currencies.AsNoTracking().CountAsync();
            var expectedPages = expectedTotal == 0 ? 0 : (int)Math.Ceiling(expectedTotal / 2d);

            Assert.Equal(expectedTotal, resp.TotalCount);
            Assert.Equal(expectedPages, resp.TotalPages);
            Assert.True(resp.Items.Count <= 2);
        }

        [Fact]
        public async Task ListCurrencies_FilterByName_ShouldReturnOnlyMatching()
        {
            var query = new ListCurrenciesQuery{ Name = "Euro", IsActive = null, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Single(resp.Items);
            Assert.Equal("Euro", resp.Items[0].Name);
            Assert.Equal("EUR", resp.Items[0].Code);
        }

        [Fact]
        public async Task ListCurrencies_FilterByIsActive_True_ShouldReturnOnlyActive()
        {
            var query = new ListCurrenciesQuery{ Name = null, IsActive = true, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Equal(3, resp.TotalCount);
            Assert.Equal(1, resp.TotalPages);
            Assert.Equal(3, resp.Items.Count);
            Assert.All(resp.Items, x => Assert.True(x.IsActive));
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}