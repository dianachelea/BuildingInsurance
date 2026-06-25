using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
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
    public sealed class CreateClientIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly CreateClientCommandHandler _handler;

        public CreateClientIntegrationTests()
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

            _handler = new CreateClientCommandHandler(_uow, NullLogger<CreateClientCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var existing = new Client(
                type: ClientType.Individual,
                fullName: "Existing Client",
                contactInfo: new ContactInfo("existing@mail.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null
            );

            _db.Clients.Add(existing);
            _db.SaveChanges();
        }

        [Fact]
        public async Task CreateClientValidInput_ShouldPersist()
        {
            var command = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = new()
                {
                    Street = "Main St",
                    Number = "10"
                }
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);

            var persisted = await _db.Clients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == result.Value.Id);

            Assert.NotNull(persisted);
            Assert.Equal("John Doe", persisted!.FullName);
            Assert.Equal("john@example.com", persisted.ContactInfo.Email);
            Assert.Equal("0712345678", persisted.ContactInfo.Phone);
            Assert.NotNull(persisted.ContactInfo.Address);
            Assert.Equal("MAIN ST", persisted.ContactInfo.Address!.Street);
            Assert.Equal("10", persisted.ContactInfo.Address.Number);
        }

        [Fact]
        public async Task CreateClientEmailAlreadyExists_ShouldReturnConflict_AndNotPersist()
        {
            var command = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "existing@mail.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("A client with this email already exists.", result.Error);

            var created = await _db.Clients.AsNoTracking()
                .AnyAsync(c => c.ContactInfo.Email == "existing@mail.com" && c.FullName == "John Doe");

            Assert.False(created);
        }

        [Fact]
        public async Task CreateClientIdentifierAlreadyExists_ShouldReturnConflict_AndNotPersist()
        {
            var command = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john2@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1111111111111",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("A client with this identification number already exists.", result.Error);

            var exists = await _db.Clients.AsNoTracking().AnyAsync(c => c.ContactInfo.Email == "john2@example.com");
            Assert.False(exists);
        }

        [Fact]
        public async Task CreateClientWhenCommitFails_ShouldRollback()
        {
            var optionsFail = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new SimulatedFailureInterceptor())
                .Options;

            using var failingContext = new BuildingInsuranceDbContext(optionsFail);
            await failingContext.Database.EnsureCreatedAsync();

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

            var failingHandler = new CreateClientCommandHandler(failingUow, NullLogger<CreateClientCommandHandler>.Instance);

            var command = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "Rollback Test",
                Email = "rollback@test.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "9999999999999",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = await failingHandler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during client creation.", result.Error);

            var exists = await failingContext.Clients.AsNoTracking()
                .AnyAsync(c => c.ContactInfo.Email == "rollback@test.com");

            Assert.False(exists, "Client should not exist in DB because transaction should have rolled back.");
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}