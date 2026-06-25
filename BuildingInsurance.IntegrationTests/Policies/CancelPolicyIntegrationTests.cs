using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CancelPolicy;
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
    public sealed class CancelPolicyIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly IPolicyPricingService _pricingService;
        private readonly IClock _clock;
        private readonly CreateDraftPolicyCommandHandler _createHandler;
        private readonly ActivatePolicyCommandHandler _activateHandler;
        private readonly CancelPolicyCommandHandler _handler;
        private readonly IClientBuildingVerifier _clientBuildingVerifier;
        private readonly ICurrencyVerifier _currencyVerifier;
        private readonly IBrokerVerifier _brokerVerifier;
        private Guid _clientId;
        private Guid _buildingId;
        private Guid _currencyId;
        private Guid _brokerId;

        public CancelPolicyIntegrationTests()
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

            _clock = new FixedClock(new DateTime(2026, 02, 28, 10, 0, 0, DateTimeKind.Utc));
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

            _activateHandler = new ActivatePolicyCommandHandler(
                _uow,
                _clock,
                NullLogger<ActivatePolicyCommandHandler>.Instance,
                _brokerVerifier,
                _pricingService,
                _clientBuildingVerifier,
                _currencyVerifier);

            _handler = new CancelPolicyCommandHandler(
                _uow,
                NullLogger<CancelPolicyCommandHandler>.Instance,
                _brokerVerifier);

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

            var broker = new Domain.Entities.Management.Broker(
                "BRK1",
                "Broker",
                new ContactInfo("broker@x.com", "0700"),
                BrokerStatus.Active,
                0.10m);
            _db.Brokers.Add(broker);
            _brokerId = broker.Id;

            _db.SaveChanges();
        }

        private async Task<Guid> CreateAndActivatePolicyAsync()
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

            Assert.True(create.IsSuccess, $"{create.ErrorType}: {create.Error}");

            var activate = await _activateHandler.Handle(
                new ActivatePolicyCommand(create.Value!.Id),
                CancellationToken.None);

            Assert.True(activate.IsSuccess, $"{activate.ErrorType}: {activate.Error}");

            return create.Value!.Id;
        }

        [Fact]
        public async Task CancelPolicy_WhenActiveAndValid_ShouldCancel()
        {
            var policyId = await CreateAndActivatePolicyAsync();

            _db.ChangeTracker.Clear();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var cancelContext = new BuildingInsuranceDbContext(options);
            await cancelContext.Database.EnsureCreatedAsync();

            var clientRepo = new ClientRepository(cancelContext);
            var buildingRepo = new BuildingRepository(cancelContext);
            var cityRepo = new CityRepository(cancelContext);
            var countyRepo = new CountyRepository(cancelContext);
            var countryRepo = new CountryRepository(cancelContext);
            var policyRepo = new PolicyRepository(cancelContext);
            var brokerRepo = new BrokerRepository(cancelContext);
            var currencyRepo = new CurrencyRepository(cancelContext);
            var riskRepo = new RiskFactorConfigurationRepository(cancelContext);
            var feeRepo = new FeeConfigurationRepository(cancelContext);

            var cancelUow = new UnitOfWork(cancelContext, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskRepo, feeRepo);
            var cancelBrokerVerifier = new BrokerVerifier(cancelUow, NullLogger<BrokerVerifier>.Instance);
            var cancelHandler = new CancelPolicyCommandHandler(
                cancelUow,
                NullLogger<CancelPolicyCommandHandler>.Instance,
                cancelBrokerVerifier);

            var statusCheck = await cancelContext.Policies.AsNoTracking()
                .Where(p => p.Id == policyId)
                .Select(p => p.PolicyStatus)
                .SingleAsync();

            Assert.Equal(PolicyStatus.Active, statusCheck);

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policyId,
                Reason = Domain.Constants.CancellationReasons.Allowed[0],
                CancellationEffectiveDate = new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            var result = await cancelHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess, $"{result.ErrorType}: {result.Error}");
            Assert.Equal(PolicyStatus.Cancelled, result.Value!.PolicyStatus);
            Assert.NotNull(result.Value.CancellationEffectiveDate);

            var persisted = await cancelContext.Policies.AsNoTracking()
                .FirstAsync(p => p.Id == policyId);

            Assert.Equal(PolicyStatus.Cancelled, persisted.PolicyStatus);
            Assert.NotNull(persisted.CancellationEffectiveDate);
        }

        [Fact]
        public async Task CancelPolicy_WhenNotFound_ShouldReturnNotFound()
        {
            var cmd = new CancelPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                Reason = "Customer request",
                CancellationEffectiveDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Contains("Policy with ID", result.Error);
        }

        [Fact]
        public async Task CancelPolicy_WhenBrokerInactive_ShouldReturnValidation()
        {
            var policyId = await CreateAndActivatePolicyAsync();

            var broker = await _db.Brokers.FirstAsync(b => b.Id == _brokerId);
            broker.Deactivate();
            await _db.SaveChangesAsync();

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policyId,
                Reason = "Customer request",
                CancellationEffectiveDate = new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Broker is inactive.", result.Error);
        }

        [Fact]
        public async Task CancelPolicy_WhenCommitFails_ShouldReturnGeneric_AndNotPersist()
        {
            var policyId = await CreateAndActivatePolicyAsync();

            _db.ChangeTracker.Clear();

            var optionsFail = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new SimulatedFailureInterceptor())
                .Options;

            using var failingContext = new BuildingInsuranceDbContext(optionsFail);
            await failingContext.Database.EnsureCreatedAsync();

            failingContext.Countries.Add(new Domain.Entities.Geography.Country(Guid.NewGuid(), "X"));
            await Assert.ThrowsAsync<Exception>(() => failingContext.SaveChangesAsync());
            failingContext.ChangeTracker.Clear();

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
                riskRepo,
                feeRepo);

            var failingBrokerVerifier = new BrokerVerifier(failingUow, NullLogger<BrokerVerifier>.Instance);

            var failingHandler = new CancelPolicyCommandHandler(
                failingUow,
                NullLogger<CancelPolicyCommandHandler>.Instance,
                failingBrokerVerifier);

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policyId,
                Reason = Domain.Constants.CancellationReasons.Allowed[0],
                CancellationEffectiveDate = new DateTime(2026, 04, 01, 0, 0, 0, DateTimeKind.Utc)
            };

            var result = await failingHandler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure, "Expected failure but got success.");
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("An error occurred while cancelling the policy.", result.Error);

            var persisted = await failingContext.Policies.AsNoTracking()
                .FirstAsync(p => p.Id == policyId);

            Assert.Equal(PolicyStatus.Active, persisted.PolicyStatus);
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