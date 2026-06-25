using BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients;
using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Clients
{
    public sealed class ListClientsIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListClientsHandler _handler;

        public ListClientsIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var clientRepo = new ClientRepository(_db);

            _handler = new ListClientsHandler(clientRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var c1 = new Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Charlie",
                contactInfo: new ContactInfo("charlie@x.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null);

            var c2 = new Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Alice",
                contactInfo: new ContactInfo("alice@x.com", "0700000000", null),
                personalIdentificationNumber: "2222222222222",
                companyRegistrationNumber: null);

            var c3 = new Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Bob",
                contactInfo: new ContactInfo("bob@x.com", "0700000000", null),
                personalIdentificationNumber: "3333333333333",
                companyRegistrationNumber: null);

            var c4 = new Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "David",
                contactInfo: new ContactInfo("david@x.com", "0700000000", null),
                personalIdentificationNumber: "4444444444444",
                companyRegistrationNumber: null);

            var c5 = new Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Eve",
                contactInfo: new ContactInfo("eve@x.com", "0700000000", null),
                personalIdentificationNumber: "5555555555555",
                companyRegistrationNumber: null);

            _db.Clients.AddRange(c1, c2, c3, c4, c5);
            _db.SaveChanges();
        }

        [Fact]
        public async Task ListClients_Page2_PageSize2_ShouldReturnSortedItems()
        {
            var query = new ListClientsQuery{ Name = null, Identifier = null, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("Charlie", result.Value.Items[0].FullName);
            Assert.Equal("David", result.Value.Items[1].FullName);
        }

        [Fact]
        public async Task ListClients_PageGreaterThanTotalPages_ShouldReturnEmptyItems()
        {
            var query = new ListClientsQuery{ Name = null, Identifier = null, Page = 10, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(5, result.Value.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
        }

        [Fact]
        public async Task ListClients_FilterByName_ShouldReturnOnlyMatchingClients()
        {
            var query = new ListClientsQuery{ Name = "Alice", Identifier = null, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Single(result.Value!.Items);
            Assert.Equal("Alice", result.Value.Items[0].FullName);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}