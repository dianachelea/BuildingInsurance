using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.CreateCurrency;
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
    public sealed class CreateCurrencyIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly CreateCurrencyCommandHandler _handler;

        public CreateCurrencyIntegrationTests()
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

            _handler = new CreateCurrencyCommandHandler(_uow, NullLogger<CreateCurrencyCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var eur = new Domain.Entities.Metadata.Currency(
                code: "EUR",
                name: "Euro",
                exchangeRateToBase: 1.0m,
                isActive: true);

            _db.Currencies.Add(eur);
            _db.SaveChanges();
        }

        [Fact]
        public async Task CreateCurrency_ValidInput_ShouldPersist()
        {
            var cmd = new CreateCurrencyCommand
            {
                Code = "USD",
                Name = "US Dollar",
                ExchangeRateToBase = 4.50m,
                IsActive = true
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);

            var persisted = await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == result.Value.Id);

            Assert.NotNull(persisted);
            Assert.Equal("USD", persisted!.Code);
            Assert.Equal("US Dollar", persisted.Name);
            Assert.Equal(4.50m, persisted.ExchangeRateToBase);
            Assert.True(persisted.IsActive);
        }

        [Fact]
        public async Task CreateCurrency_WhenCodeAlreadyExists_ShouldReturnConflict_AndNotPersist()
        {
            var cmd = new CreateCurrencyCommand
            {
                Code = "EUR",
                Name = "Euro Duplicate",
                ExchangeRateToBase = 1.0m,
                IsActive = true
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Currency code already exists.", result.Error);

            var exists = await _db.Currencies.AsNoTracking().AnyAsync(c => c.Name == "Euro Duplicate");

            Assert.False(exists);
        }

        [Fact]
        public async Task CreateCurrency_WhenCommitFails_ShouldRollback()
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

            var failingUow = new UnitOfWork(failingContext, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskFactorConfigRepo, feeConfigRepo);

            var failingHandler = new CreateCurrencyCommandHandler(
                failingUow,
                NullLogger<CreateCurrencyCommandHandler>.Instance
            );

            var existingCodes = await _db.Currencies.AsNoTracking().Select(c => c.Code).ToListAsync();

            var cmd = new CreateCurrencyCommand
            {
                Code = "RON",
                Name = "Rollback Currency",
                ExchangeRateToBase = 5.10m,
                IsActive = true
            };

            var result = await failingHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during currency creation.", result.Error);

            var exists = await failingContext.Currencies.AsNoTracking().AnyAsync(c => c.Code == "RON");

            Assert.False(exists, "Currency should not exist in DB because commit failed and transaction rolled back.");
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}