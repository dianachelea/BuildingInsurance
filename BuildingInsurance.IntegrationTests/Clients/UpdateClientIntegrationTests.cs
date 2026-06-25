using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Clients.Commands.UpdateClient;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
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

namespace BuildingInsurance.IntegrationTests.Clients
{
    public sealed class UpdateClientIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly UpdateClientCommandHandler _handler;

        private Guid _clientId;

        public UpdateClientIntegrationTests()
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

            _handler = new UpdateClientCommandHandler(_uow, NullLogger<UpdateClientCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var client1 = new Client(
                type: ClientType.Individual,
                fullName: "Client One",
                contactInfo: new ContactInfo("one@mail.com", "0700000001", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null
            );

            var client2 = new Client(
                type: ClientType.Individual,
                fullName: "Client Two",
                contactInfo: new ContactInfo("two@mail.com", "0700000002", null),
                personalIdentificationNumber: "2222222222222",
                companyRegistrationNumber: null
            );

            _db.Clients.AddRange(client1, client2);
            _db.SaveChanges();

            _clientId = client1.Id;
        }

        [Fact]
        public async Task UpdateClient_WhenNotFound_ShouldReturnNotFound()
        {
            var command = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "Updated",
                Email = "updated@mail.com",
                Phone = "0799999999",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Client not found.", result.Error);
        }

        [Fact]
        public async Task UpdateClient_WhenIdentifierConflictsForOtherClient_ShouldReturnConflict_AndNotPersistChanges()
        {
            var command = new UpdateClientCommand
            {
                ClientId = _clientId,
                FullName = "Updated Name",
                Email = "updated@mail.com",
                Phone = "0799999999",
                Address = null,
                IdentificationNumber = "  2222222222222  ",
                IdentificationChangeReason = "Fix typo"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Identification number already exists.", result.Error);

            var persisted = await _db.Clients.AsNoTracking().FirstAsync(c => c.Id == _clientId);
            Assert.Equal("Client One", persisted.FullName);
            Assert.Equal("one@mail.com", persisted.ContactInfo.Email);
        }

        [Fact]
        public async Task UpdateClient_ValidInput_ShouldPersist()
        {
            var command = new UpdateClientCommand
            {
                ClientId = _clientId,
                FullName = "Updated Name",
                Email = "updated@mail.com",
                Phone = "0799999999",
                Address = new()
                {
                    Street = "New Street",
                    Number = "10"
                },
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal(_clientId, result.Value!.Id);

            var persisted = await _db.Clients.AsNoTracking().FirstAsync(c => c.Id == _clientId);
            Assert.Equal("Updated Name", persisted.FullName);
            Assert.Equal("updated@mail.com", persisted.ContactInfo.Email);
            Assert.Equal("0799999999", persisted.ContactInfo.Phone);
            Assert.NotNull(persisted.ContactInfo.Address);
            Assert.Equal("NEW STREET", persisted.ContactInfo.Address!.Street);
            Assert.Equal("10", persisted.ContactInfo.Address.Number);
        }

        [Fact]
        public async Task UpdateClient_WhenCommitFails_ShouldRollback()
        {
            var seedOptions = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            Guid clientId;

            using (var seedContext = new BuildingInsuranceDbContext(seedOptions))
            {
                await seedContext.Database.EnsureCreatedAsync();

                var seededClient = new Client(
                    type: ClientType.Individual,
                    fullName: "Client Rollback",
                    contactInfo: new ContactInfo("rollback@x.com", "0700000000", null),
                    personalIdentificationNumber: "3333333333333",
                    companyRegistrationNumber: null
                );

                seedContext.Clients.Add(seededClient);
                await seedContext.SaveChangesAsync();

                clientId = seededClient.Id;
            }

            var optionsFail = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new SimulatedFailureInterceptor())
                .Options;

            using var failingContext = new BuildingInsuranceDbContext(optionsFail);

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

            var failingUow = new UnitOfWork(
                failingContext,
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

            var failingHandler = new UpdateClientCommandHandler(failingUow, NullLogger<UpdateClientCommandHandler>.Instance);

            var command = new UpdateClientCommand
            {
                ClientId = clientId,
                FullName = "Should Not Persist",
                Email = "no@persist.com",
                Phone = "0799999999",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = await failingHandler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during client update.", result.Error);

            var persisted = await failingContext.Clients.AsNoTracking().FirstAsync(c => c.Id == clientId);
            Assert.Equal("Client Rollback", persisted.FullName);
            Assert.Equal("rollback@x.com", persisted.ContactInfo.Email);
            Assert.Equal("0700000000", persisted.ContactInfo.Phone);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}