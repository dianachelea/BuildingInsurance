using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.GetCurrencyById;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.BuildingsRepository;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using BuildingInsurance.Infrastructure.Repositories.ManagementRepository;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Currencies
{
    public sealed class GetCurrencyIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly GetCurrencyByIdHandler _handler;

        private readonly Guid _eurId = Guid.NewGuid();
        private readonly Guid _usdId = Guid.NewGuid();

        public GetCurrencyIntegrationTests()
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

            _handler = new GetCurrencyByIdHandler(_uow);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var eur = new Domain.Entities.Metadata.Currency(
                code: "EUR",
                name: "Euro",
                exchangeRateToBase: 1.0m,
                isActive: true);

            var usd = new Domain.Entities.Metadata.Currency(
                code: "USD",
                name: "US Dollar",
                exchangeRateToBase: 4.50m,
                isActive: false);

            typeof(Domain.Entities.Metadata.Currency)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(eur, _eurId);

            typeof(Domain.Entities.Metadata.Currency)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(usd, _usdId);

            _db.Currencies.AddRange(eur, usd);
            _db.SaveChanges();
        }

        [Fact]
        public async Task GetCurrencyById_WhenNotFound_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();
            var query = new GetCurrencyByIdQuery(missingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Currency not found.", result.Error);
        }

        [Fact]
        public async Task GetCurrencyById_WhenExists_ShouldReturnDto()
        {
            var query = new GetCurrencyByIdQuery(_eurId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var dto = result.Value!;
            Assert.Equal(_eurId, dto.Id);
            Assert.Equal("EUR", dto.Code);
            Assert.Equal("Euro", dto.Name);
            Assert.Equal(1.0m, dto.ExchangeRateToBase);
            Assert.True(dto.IsActive);

            var persisted = await _db.Currencies.AsNoTracking().FirstAsync(c => c.Id == _eurId);
            Assert.Equal("Euro", persisted.Name);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}