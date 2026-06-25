using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.UpdateBroker;
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
    public sealed class UpdateBrokerIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly UpdateBrokerCommandHandler _handler;

        private Guid _brokerId;

        public UpdateBrokerIntegrationTests()
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
            _handler = new UpdateBrokerCommandHandler(_uow, NullLogger<UpdateBrokerCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var broker1 = new Domain.Entities.Management.Broker("BRK-U1", "Broker One", new ContactInfo("one@b.com", "0700001001"), BrokerStatus.Active, 0.10m);
            var broker2 = new Domain.Entities.Management.Broker("BRK-U2", "Broker Two", new ContactInfo("two@b.com", "0700001002"), BrokerStatus.Active, 0.12m);

            _db.Brokers.AddRange(broker1, broker2);
            _db.SaveChanges();

            _brokerId = broker1.Id;
        }

        [Fact]
        public async Task UpdateBroker_WhenNotFound_ShouldReturnNotFound()
        {
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "Updated",
                Email = "updated@b.com",
                Phone = "0700009999",
                CommissionPercentage = 0.20m
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Broker with ID {cmd.Id} not found.", result.Error);
        }

        [Fact]
        public async Task UpdateBroker_WhenEmailConflicts_ShouldReturnConflict_AndNotPersist()
        {
            var cmd = new UpdateBrokerCommand
            {
                Id = _brokerId,
                FullName = "Broker One Updated",
                Email = "two@b.com",
                Phone = "0700002000",
                CommissionPercentage = 0.30m
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another broker already exists with the same email address.", result.Error);

            var persisted = await _db.Brokers.AsNoTracking().FirstAsync(b => b.Id == _brokerId);
            Assert.Equal("Broker One", persisted.FullName);
            Assert.Equal("one@b.com", persisted.ContactInfo.Email);
        }

        [Fact]
        public async Task UpdateBroker_ValidInput_ShouldPersist()
        {
            var cmd = new UpdateBrokerCommand
            {
                Id = _brokerId,
                FullName = "Broker One Updated",
                Email = "one.updated@b.com",
                Phone = "0700002000",
                CommissionPercentage = 0.25m
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var persisted = await _db.Brokers.AsNoTracking().FirstAsync(b => b.Id == _brokerId);
            Assert.Equal("Broker One Updated", persisted.FullName);
            Assert.Equal("one.updated@b.com", persisted.ContactInfo.Email);
            Assert.Equal("0700002000", persisted.ContactInfo.Phone);
            Assert.Equal(0.25m, persisted.CommissionPercentage);
        }

        [Fact]
        public async Task UpdateBroker_WhenCommitFails_ShouldRollback()
        {
            var cmd = new UpdateBrokerCommand
            {
                Id = _brokerId,
                FullName = "Should Not Persist",
                Email = "no.persist@b.com",
                Phone = "0700003333",
                CommissionPercentage = 0.99m
            };

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

            var failingUow = new UnitOfWork(failingContext, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskFactorRepo, feeRepo);
            var failingHandler = new UpdateBrokerCommandHandler(failingUow, NullLogger<UpdateBrokerCommandHandler>.Instance);

            var result = await failingHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error while updating broker.", result.Error);

            var updatedExists = await failingContext.Brokers.AsNoTracking()
                .AnyAsync(b => b.Id == _brokerId && b.FullName == "Should Not Persist");

            Assert.False(updatedExists);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}