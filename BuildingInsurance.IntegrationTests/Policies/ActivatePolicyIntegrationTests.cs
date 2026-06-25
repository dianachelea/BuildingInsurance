using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy;
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
    public sealed class ActivatePolicyIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;

        private readonly IPolicyPricingService _pricingService;
        private readonly IClock _clock;
        private readonly IClientBuildingVerifier _clientBuildingVerifier;
        private readonly ICurrencyVerifier _currencyVerifier;
        private readonly IBrokerVerifier _brokerVerifier;

        private readonly CreateDraftPolicyCommandHandler _createHandler;
        private readonly ActivatePolicyCommandHandler _handler;

        private Guid _clientId;
        private Guid _buildingId;
        private Guid _currencyId;
        private Guid _brokerId;

        public ActivatePolicyIntegrationTests()
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

            _createHandler = new CreateDraftPolicyCommandHandler(
                _uow,
                _pricingService,
                NullLogger<CreateDraftPolicyCommandHandler>.Instance,
                _clientBuildingVerifier,
                _currencyVerifier,
                _brokerVerifier);

            _handler = new ActivatePolicyCommandHandler(
                _uow,
                _clock,
                NullLogger<ActivatePolicyCommandHandler>.Instance,
                _brokerVerifier,
                _pricingService,
                _clientBuildingVerifier,
                _currencyVerifier);

            Seed();
        }

        private void Seed()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);

            var city = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);
            _db.Cities.Add(city);

            var client = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Client",
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

            var currency = new Domain.Entities.Metadata.Currency("EUR", "Euro", 1.0m, true);
            _db.Currencies.Add(currency);
            _currencyId = currency.Id;

            var broker = new Domain.Entities.Management.Broker("BRK1", "Broker", new ContactInfo("broker@x.com", "0700"), BrokerStatus.Active, 0.10m);
            _db.Brokers.Add(broker);
            _brokerId = broker.Id;

            _db.SaveChanges();
        }

        [Fact]
        public async Task ActivatePolicy_WhenDraftAndValid_ShouldActivate()
        {
            var create = await _createHandler.Handle(new CreateDraftPolicyCommand
            {
                ClientId = _clientId,
                BuildingId = _buildingId,
                BrokerId = _brokerId,
                CurrencyId = _currencyId,
                BasePremium = 1000m,
                StartDate = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 03, 01, 0, 0, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            Assert.True(create.IsSuccess);
            var policyId = create.Value!.Id;

            var result = await _handler.Handle(new ActivatePolicyCommand(policyId), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(PolicyStatus.Active, result.Value!.PolicyStatus);

            var persisted = await _db.Policies.AsNoTracking().FirstAsync(p => p.Id == policyId);
            Assert.Equal(PolicyStatus.Active, persisted.PolicyStatus);
        }

        [Fact]
        public async Task ActivatePolicy_WhenBrokerInactive_ShouldReturnValidation()
        {
            var create = await _createHandler.Handle(new CreateDraftPolicyCommand
            {
                ClientId = _clientId,
                BuildingId = _buildingId,
                BrokerId = _brokerId,
                CurrencyId = _currencyId,
                BasePremium = 1000m,
                StartDate = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            }, CancellationToken.None);

            Assert.True(create.IsSuccess);

            var broker = await _db.Brokers.FirstAsync(b => b.Id == _brokerId);
            broker.Deactivate();
            await _db.SaveChangesAsync();

            var result = await _handler.Handle(new ActivatePolicyCommand(create.Value!.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Broker is inactive.", result.Error);
        }

        [Fact]
        public async Task ActivatePolicy_WhenNotFound_ShouldReturnNotFound()
        {
            var result = await _handler.Handle(new ActivatePolicyCommand(Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Contains("Policy with ID", result.Error);
        }

        [Fact]
        public async Task ActivatePolicy_WhenCommitFails_ShouldReturnGeneric_AndNotPersist()
        {
            var create = await _createHandler.Handle(new CreateDraftPolicyCommand
            {
                ClientId = _clientId,
                BuildingId = _buildingId,
                BrokerId = _brokerId,
                CurrencyId = _currencyId,
                BasePremium = 1000m,
                StartDate = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 03, 01, 0, 0, 0, DateTimeKind.Utc)
            }, CancellationToken.None);

            var policyId = create.Value!.Id;

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

            var failingPricingService = new PolicyPricingService(failingSelector);
            var failingClientBuildingVerifier = new ClientBuildingVerifier(failingUow, NullLogger<ClientBuildingVerifier>.Instance);
            var failingCurrencyVerifier = new CurrencyVerifier(failingUow, NullLogger<CurrencyVerifier>.Instance);
            var failingBrokerVerifier = new BrokerVerifier(failingUow, NullLogger<BrokerVerifier>.Instance);

            var failingHandler = new ActivatePolicyCommandHandler(
                failingUow,
                _clock,
                NullLogger<ActivatePolicyCommandHandler>.Instance,
                failingBrokerVerifier,
                failingPricingService,
                failingClientBuildingVerifier,
                failingCurrencyVerifier);

            var result = await failingHandler.Handle(new ActivatePolicyCommand(policyId), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("An error occurred while activating the policy.", result.Error);

            var persisted = await failingContext.Policies.AsNoTracking().FirstAsync(p => p.Id == policyId);
            Assert.Equal(PolicyStatus.Draft, persisted.PolicyStatus);
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