using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.ActivateBroker;
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
    public sealed class ActivateBrokerIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly ActivateBrokerCommandHandler _handler;

        private Guid _inactiveBrokerId;

        public ActivateBrokerIntegrationTests()
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
            _handler = new ActivateBrokerCommandHandler(_uow, NullLogger<ActivateBrokerCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var broker = new Domain.Entities.Management.Broker("BRK-INACT", "Inactive Broker", new ContactInfo("inactive@b.com", "0700000000"), BrokerStatus.Inactive, 0.10m);
            _db.Brokers.Add(broker);
            _db.SaveChanges();

            _inactiveBrokerId = broker.Id;
        }

        [Fact]
        public async Task ActivateBroker_WhenNotFound_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();
            var cmd = new ActivateBrokerCommand(missingId);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Broker with ID {missingId} not found.", result.Error);
        }

        [Fact]
        public async Task ActivateBroker_WhenInactive_ShouldActivate_AndPersist()
        {
            var cmd = new ActivateBrokerCommand(_inactiveBrokerId);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(BrokerStatus.Active, result.Value!.BrokerStatus);

            var persisted = await _db.Brokers.AsNoTracking().FirstAsync(b => b.Id == _inactiveBrokerId);
            Assert.Equal(BrokerStatus.Active, persisted.BrokerStatus);
        }

        [Fact]
        public async Task ActivateBroker_WhenCommitFails_ShouldReturnGeneric_AndNotPersist()
        {
            var seedOptions = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            Guid brokerId;

            using (var seedContext = new BuildingInsuranceDbContext(seedOptions))
            {
                await seedContext.Database.EnsureCreatedAsync();

                var broker = new Domain.Entities.Management.Broker(
                    "BRK-INACT2",
                    "Inactive Broker 2",
                    new ContactInfo("inactive2@b.com", "0700000009"),
                    BrokerStatus.Inactive,
                    0.10m);

                seedContext.Brokers.Add(broker);
                await seedContext.SaveChangesAsync();

                brokerId = broker.Id;
            }

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

            var failingHandler = new ActivateBrokerCommandHandler(failingUow, NullLogger<ActivateBrokerCommandHandler>.Instance);

            var result = await failingHandler.Handle(new ActivateBrokerCommand(brokerId), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during broker activation.", result.Error);

            var persisted = await failingContext.Brokers.AsNoTracking().FirstAsync(b => b.Id == brokerId);
            Assert.Equal(BrokerStatus.Inactive, persisted.BrokerStatus);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}