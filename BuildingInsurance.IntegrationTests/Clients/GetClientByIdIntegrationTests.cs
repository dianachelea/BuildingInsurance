using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BuildingInsurance.Application.Features.Brokers.Clients.Queries.GetClient;

namespace BuildingInsurance.IntegrationTests.Clients
{
    public sealed class GetClientByIdIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IClientRepository _clientRepository;
        private readonly GetClientByIdHandler _handler;

        private readonly Guid _clientWithoutAddressId = Guid.NewGuid();
        private readonly Guid _clientWithAddressId = Guid.NewGuid();

        public GetClientByIdIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            _clientRepository = new ClientRepository(_db);
            _handler = new GetClientByIdHandler(_clientRepository);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var clientWithoutAddress = new Client(
                id: _clientWithoutAddressId,
                type: ClientType.Individual,
                fullName: "No Address Client",
                contactInfo: new ContactInfo("noaddress@mail.com", "0700000000", address: null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null
            );

            var clientWithAddress = new Client(
                id: _clientWithAddressId,
                type: ClientType.Individual,
                fullName: "With Address Client",
                contactInfo: new ContactInfo("withaddress@mail.com", "0711111111", new Address("Main St", "10")),
                personalIdentificationNumber: "2222222222222",
                companyRegistrationNumber: null
            );

            _db.Clients.AddRange(clientWithoutAddress, clientWithAddress);
            _db.SaveChanges();
        }

        [Fact]
        public async Task GetClientById_WhenClientDoesNotExist_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();
            var query = new GetClientByIdQuery(missingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Client with id {missingId} not found.", result.Error);
        }

        [Fact]
        public async Task GetClientById_WhenClientExists_WithoutAddress_ShouldReturnClientDetails()
        {
            var query = new GetClientByIdQuery(_clientWithoutAddressId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            Assert.Equal(_clientWithoutAddressId, result.Value!.Id);
            Assert.Equal(ClientType.Individual, result.Value.Type);
            Assert.Equal("No Address Client", result.Value.FullName);
            Assert.Equal("1111111111111", result.Value.PersonalIdentificationNumber);
            Assert.Null(result.Value.CompanyRegistrationNumber);
            Assert.Equal("noaddress@mail.com", result.Value.Email);
            Assert.Equal("0700000000", result.Value.Phone);
            Assert.Null(result.Value.Address);

            var persisted = await _db.Clients.AsNoTracking().FirstAsync(c => c.Id == _clientWithoutAddressId);
            Assert.Equal("No Address Client", persisted.FullName);
            Assert.Null(persisted.ContactInfo.Address);
        }

        [Fact]
        public async Task GetClientById_WhenClientExists_WithAddress_ShouldReturnClientDetailsIncludingAddress()
        {
            var query = new GetClientByIdQuery(_clientWithAddressId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            Assert.Equal(_clientWithAddressId, result.Value!.Id);
            Assert.Equal("With Address Client", result.Value.FullName);
            Assert.Equal("withaddress@mail.com", result.Value.Email);
            Assert.Equal("0711111111", result.Value.Phone);

            Assert.NotNull(result.Value.Address);
            Assert.Equal("MAIN ST", result.Value.Address!.Street);
            Assert.Equal("10", result.Value.Address.Number);

            var persisted = await _db.Clients.AsNoTracking().FirstAsync(c => c.Id == _clientWithAddressId);
            Assert.NotNull(persisted.ContactInfo.Address);
        }
        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}