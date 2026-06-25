using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Brokers.Policies.Services;
using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Selection;
using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies;
using BuildingInsurance.Application.Features.Common.Abstractions;
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

namespace BuildingInsurance.IntegrationTests.Policies
{
    public sealed class CreateDraftPolicyIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;

        private readonly IPolicyPricingService _pricingService;
        private readonly IClock _clock;
        private readonly CreateDraftPolicyCommandHandler _handler;
        private readonly IClientBuildingVerifier _clientBuildingVerifier;
        private readonly ICurrencyVerifier _currencyVerifier;
        private readonly IBrokerVerifier _brokerVerifier;

        private Guid _countryId;
        private Guid _countyId;
        private Guid _cityId;
        private Guid _clientId;
        private Guid _buildingId;
        private Guid _currencyId;
        private Guid _brokerId;

        public CreateDraftPolicyIntegrationTests()
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
            var riskRepo = new RiskFactorConfigurationRepository(_db);
            var feeRepo = new FeeConfigurationRepository(_db);

            _uow = new UnitOfWork(_db, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskRepo, feeRepo);

            var draftStrategy = new DraftPolicyPricingStrategy(_uow);
            var snapshotStrategy = new SnapshotPolicyPricingStrategy();

            var selector = new PolicyPricingStrategySelector(new IPolicyPricingStrategy[]
            {
                draftStrategy,
                snapshotStrategy
            });

            _pricingService = new PolicyPricingService(selector);
            _clock = new FixedClock(new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc));
            _clientBuildingVerifier = new ClientBuildingVerifier(_uow, NullLogger<ClientBuildingVerifier>.Instance);
            _currencyVerifier = new CurrencyVerifier(_uow, NullLogger<CurrencyVerifier>.Instance);
            _brokerVerifier = new BrokerVerifier(_uow, NullLogger<BrokerVerifier>.Instance);
            _handler = new CreateDraftPolicyCommandHandler(
                _uow,
                _pricingService,
                NullLogger<CreateDraftPolicyCommandHandler>.Instance,
                _clientBuildingVerifier,
                _currencyVerifier,
                _brokerVerifier);

            Seed();
        }

        private void Seed()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);
            _countryId = country.Id;

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);
            _countyId = county.Id;

            var city = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);
            _db.Cities.Add(city);
            _cityId = city.Id;

            var client = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Policy Client",
                contactInfo: new ContactInfo("client@x.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null);
            _db.Clients.Add(client);
            _clientId = client.Id;

            var building = new Domain.Entities.Buildings.Building(
                id: Guid.NewGuid(),
                clientId: client.Id,
                address: new Address("Main St", "1"),
                cityId: city.Id,
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 80m,
                insuredValue: 100_000m,
                riskIndicators: RiskIndicators.None);
            _db.Buildings.Add(building);
            _buildingId = building.Id;

            var currency = new Domain.Entities.Metadata.Currency(
                code: "EUR",
                name: "Euro",
                exchangeRateToBase: 1.0m,
                isActive: true);
            _db.Currencies.Add(currency);
            _currencyId = currency.Id;

            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BRK1",
                name: "Broker One",
                contactInfo: new ContactInfo("broker@x.com", "0700000001"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);
            _db.Brokers.Add(broker);
            _brokerId = broker.Id;

            _db.SaveChanges();
        }

        [Fact]
        public async Task CreateDraftPolicy_ValidInput_ShouldPersist_AndSetPricing()
        {
            var start = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 03, 01, 0, 0, 0, DateTimeKind.Utc);

            var cmd = new CreateDraftPolicyCommand
            {
                ClientId = _clientId,
                BuildingId = _buildingId,
                BrokerId = _brokerId,
                CurrencyId = _currencyId,
                BasePremium = 1000m,
                StartDate = start,
                EndDate = end
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);
            Assert.Equal(PolicyStatus.Draft, result.Value.PolicyStatus);
            Assert.True(result.Value.EstimatedFinalPremium > 0m);
            Assert.Equal(0m, result.Value.FinalPremium);

            var persisted = await _db.Policies.AsNoTracking().FirstAsync(p => p.Id == result.Value.Id);
            Assert.Equal(0m, persisted.FinalPremium);
            Assert.Equal(PolicyStatus.Draft, persisted.PolicyStatus);
        }

        [Fact]
        public async Task CreateDraftPolicy_ClientNotFound_ShouldReturnNotFound()
        {
            var cmd = new CreateDraftPolicyCommand
            {
                ClientId = Guid.NewGuid(),
                BuildingId = _buildingId,
                BrokerId = _brokerId,
                CurrencyId = _currencyId,
                BasePremium = 1000m,
                StartDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddYears(1), DateTimeKind.Utc),
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Contains("Client with ID", result.Error);
        }

        [Fact]
        public async Task CreateDraftPolicy_BuildingDoesNotBelongToClient_ShouldReturnValidation()
        {
            var otherClient = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Other Client",
                contactInfo: new ContactInfo("other@x.com", "0700000999", null),
                personalIdentificationNumber: "2222222222222",
                companyRegistrationNumber: null);
            _db.Clients.Add(otherClient);
            await _db.SaveChangesAsync();

            var cmd = new CreateDraftPolicyCommand
            {
                ClientId = otherClient.Id,
                BuildingId = _buildingId,
                BrokerId = _brokerId,
                CurrencyId = _currencyId,
                BasePremium = 1000m,
                StartDate = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 03, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Building does not belong to the given client.", result.Error);
        }

        [Fact]
        public async Task CreateDraftPolicy_WhenCommitFails_ShouldRollback()
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
            var riskRepo = new RiskFactorConfigurationRepository(failingContext);
            var feeRepo = new FeeConfigurationRepository(failingContext);

            var failingUow = new UnitOfWork(failingContext, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskRepo, feeRepo);
            var failingDraftStrategy = new DraftPolicyPricingStrategy(failingUow);
            var failingSnapshotStrategy = new SnapshotPolicyPricingStrategy();

            var failingSelector = new PolicyPricingStrategySelector(new IPolicyPricingStrategy[]
            {
                failingDraftStrategy,
                failingSnapshotStrategy
            });
            var pricing = new PolicyPricingService(failingSelector);
            var clientBuildingVerifier = new ClientBuildingVerifier(failingUow, NullLogger<ClientBuildingVerifier>.Instance);
            var currencyVerifier = new CurrencyVerifier(failingUow, NullLogger<CurrencyVerifier>.Instance);
            var brokerVerifier = new BrokerVerifier(failingUow, NullLogger<BrokerVerifier>.Instance);
            var handler = new CreateDraftPolicyCommandHandler(
                failingUow,
                pricing,
                NullLogger<CreateDraftPolicyCommandHandler>.Instance,
                clientBuildingVerifier,
                currencyVerifier,
                brokerVerifier);

            var cmd = new CreateDraftPolicyCommand
            {
                ClientId = _clientId,
                BuildingId = _buildingId,
                BrokerId = _brokerId,
                CurrencyId = _currencyId,
                BasePremium = 1000m,
                StartDate = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            };

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during policy creation.", result.Error);

            var exists = await failingContext.Policies.AsNoTracking()
                .AnyAsync(p => p.ClientId == _clientId && p.BuildingId == _buildingId && p.BasePremium == 1000m);

            Assert.False(exists, "Policy should not exist because transaction should have rolled back.");
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        private sealed class FixedClock : IClock
        {
            public DateTime UtcNow { get; }
            public DateOnly TodayUtc { get; }

            public FixedClock(DateTime utcNow)
            {
                UtcNow = utcNow;
                TodayUtc = DateOnly.FromDateTime(utcNow);
            }
        }
    }
}