using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.CreateBroker;
using BuildingInsurance.Application.Features.Common.Result;
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

namespace BuildingInsurance.IntegrationTests.Broker
{
    public sealed class CreateBrokerIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly CreateBrokerCommandHandler _handler;

        public CreateBrokerIntegrationTests()
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
            var riskFactorRepo = new RiskFactorConfigurationRepository(_db);
            var feeRepo = new FeeConfigurationRepository(_db);

            _uow = new UnitOfWork(_db, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskFactorRepo, feeRepo);

            _handler = new CreateBrokerCommandHandler(_uow, NullLogger<CreateBrokerCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var existing = new Domain.Entities.Management.Broker(
                brokerCode: "BRK-001",
                name: "Existing Broker",
                contactInfo: new ContactInfo("existing@broker.com", "0700000000"),
                brokerStatus: BrokerStatus.Inactive,
                commissionPercentage: 0.10m);

            _db.Brokers.Add(existing);
            _db.SaveChanges();
        }

        [Fact]
        public async Task CreateBroker_ValidInput_ShouldPersist()
        {
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BRK-002",
                FullName = "New Broker",
                Email = "new@broker.com",
                Phone = "+40711111111",
                CommissionPercentage = 0.15m
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);

            var persisted = await _db.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == result.Value.Id);

            Assert.NotNull(persisted);
            Assert.Equal("BRK-002", persisted!.BrokerCode);
            Assert.Equal("New Broker", persisted.FullName);
            Assert.Equal("new@broker.com", persisted.ContactInfo.Email);
            Assert.Equal("+40711111111", persisted.ContactInfo.Phone);
            Assert.Equal(BrokerStatus.Inactive, persisted.BrokerStatus);
            Assert.Equal(0.15m, persisted.CommissionPercentage);
        }

        [Fact]
        public async Task CreateBroker_WhenBrokerCodeExists_ShouldReturnConflict()
        {
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BRK-001",
                FullName = "Dup Broker",
                Email = "dup@broker.com",
                Phone = "0700000001",
                CommissionPercentage = null
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another broker already exists with the same broker code.", result.Error);

            var exists = await _db.Brokers.AsNoTracking().AnyAsync(b => b.ContactInfo.Email == "dup@broker.com");
            Assert.False(exists);
        }

        [Fact]
        public async Task CreateBroker_WhenEmailExists_ShouldReturnConflict()
        {
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BRK-999",
                FullName = "Dup Email Broker",
                Email = "existing@broker.com",
                Phone = "0700000002",
                CommissionPercentage = null
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another broker already exists with the same email address.", result.Error);

            var exists = await _db.Brokers.AsNoTracking().AnyAsync(b => b.BrokerCode == "BRK-999");
            Assert.False(exists);
        }

        [Fact]
        public async Task CreateBroker_WhenCommitFails_ShouldRollback()
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
            var riskFactorRepo = new RiskFactorConfigurationRepository(failingContext);
            var feeRepo = new FeeConfigurationRepository(failingContext);

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
                riskFactorRepo,
                feeRepo);

            var failingHandler = new CreateBrokerCommandHandler(failingUow, NullLogger<CreateBrokerCommandHandler>.Instance);

            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BRK-ROLLBACK",
                FullName = "Rollback Broker",
                Email = "rollback@broker.com",
                Phone = "0700000999",
                CommissionPercentage = 0.12m
            };

            var result = await failingHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during broker creation.", result.Error);

            var exists = await failingContext.Brokers.AsNoTracking().AnyAsync(b => b.BrokerCode == "BRK-ROLLBACK" || b.ContactInfo.Email == "rollback@broker.com");

            Assert.False(exists);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}