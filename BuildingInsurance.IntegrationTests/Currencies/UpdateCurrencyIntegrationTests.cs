using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.UpdateCurrency;
using BuildingInsurance.Application.Features.Common.Result;
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

namespace BuildingInsurance.IntegrationTests.Currencies
{
    public sealed class UpdateCurrencyIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly UpdateCurrencyCommandHandler _handler;

        private Guid _currencyId;

        public UpdateCurrencyIntegrationTests()
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

            _uow = new UnitOfWork(_db, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskFactorConfigRepo, feeConfigRepo);

            _handler = new UpdateCurrencyCommandHandler(_uow, NullLogger<UpdateCurrencyCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var usd = new Domain.Entities.Metadata.Currency(
                code: "USD",
                name: "US Dollar",
                exchangeRateToBase: 4.50m,
                isActive: true);

            _db.Currencies.Add(usd);
            _db.SaveChanges();

            _currencyId = usd.Id;
        }

        [Fact]
        public async Task UpdateCurrency_WhenNotFound_ShouldReturnNotFound()
        {
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = "Updated",
                ExchangeRateToBase = 4.60m,
                IsActive = true
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Currency with ID {cmd.Id} not found.", result.Error);
        }

        [Fact]
        public async Task UpdateCurrency_ValidInput_ShouldPersist()
        {
            var cmd = new UpdateCurrencyCommand
            {
                Id = _currencyId,
                Name = "US Dollar Updated",
                ExchangeRateToBase = 4.80m,
                IsActive = false
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var persisted = await _db.Currencies.AsNoTracking().FirstAsync(c => c.Id == _currencyId);
            Assert.Equal("US Dollar Updated", persisted.Name);
            Assert.Equal(4.80m, persisted.ExchangeRateToBase);
            Assert.False(persisted.IsActive);
            Assert.Equal("USD", persisted.Code);
        }

        [Fact]

        public async Task UpdateCurrency_WhenCommitFails_ShouldRollback()
        {
            var cmd = new UpdateCurrencyCommand
            {
                Id = _currencyId,
                Name = "Should Not Persist",
                ExchangeRateToBase = 9.99m,
                IsActive = true
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

            var failingHandler = new UpdateCurrencyCommandHandler(
                failingUow,
                NullLogger<UpdateCurrencyCommandHandler>.Instance
            );

            var result = await failingHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error while updating currency.", result.Error);

            var updatedExists = await failingContext.Currencies.AsNoTracking().AnyAsync(c => c.Id == _currencyId && c.Name == "Should Not Persist" && c.ExchangeRateToBase == 9.99m);

            Assert.False(updatedExists);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}