using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries.GetPolicyById;
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
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Policies
{
    public sealed class GetPolicyByIdIntegrationTests : IDisposable
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
        private readonly GetPolicyByIdHandler _handler;

        private Guid _clientId;
        private Guid _buildingId;
        private Guid _currencyId;
        private Guid _brokerId;
        private Guid _cityId;
        private Guid _countyId;
        private Guid _countryId;

        public GetPolicyByIdIntegrationTests()
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

            var geo = new DbGeographyHostedService(_db);
            _handler = new GetPolicyByIdHandler(_uow, geo, _clientBuildingVerifier, _pricingService, _clock);

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
                fullName: "Client",
                contactInfo: new ContactInfo("client@x.com", "0700000000", new Address("Str", "1")),
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
        public async Task GetPolicyById_WhenMissing_ShouldReturnNotFound()
        {
            var result = await _handler.Handle(new GetPolicyByIdQuery(Guid.NewGuid()), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Contains("Policy with ID", result.Error);
        }

        [Fact]
        public async Task GetPolicyById_WhenExists_ShouldReturnDetails_WithGeography()
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

            var result = await _handler.Handle(new GetPolicyByIdQuery(create.Value!.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(create.Value.Id, result.Value!.Id);
            Assert.Equal(PolicyStatus.Draft, result.Value.PolicyStatus);
            Assert.Equal("EUR", result.Value.Currency);

            Assert.NotNull(result.Value.Client);
            Assert.Equal(_clientId, result.Value.Client.Id);

            Assert.NotNull(result.Value.Building);
            Assert.Equal(_buildingId, result.Value.Building.Id);
            Assert.Equal("CLUJ-NAPOCA", result.Value.Building.City);
            Assert.Equal("CLUJ", result.Value.Building.County);
            Assert.Equal("ROMANIA", result.Value.Building.Country);

            Assert.NotNull(result.Value.AppliedFees);
            Assert.NotNull(result.Value.AppliedRiskFactors);
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

        private sealed class DbGeographyHostedService : IGeographyCachingService
        {
            private readonly BuildingInsuranceDbContext _db;
            public DbGeographyHostedService(BuildingInsuranceDbContext db) => _db = db;

            public Task LoadAsync(CancellationToken ct) => Task.CompletedTask;

            public bool TryGet(Guid cityId, out string city, out string county, out string country)
            {
                var cityEntity = _db.Cities.AsNoTracking().FirstOrDefault(c => c.Id == cityId);
                if (cityEntity is null)
                {
                    city = county = country = string.Empty;
                    return false;
                }

                var countyEntity = _db.Counties.AsNoTracking().FirstOrDefault(c => c.Id == cityEntity.CountyId);
                if (countyEntity is null)
                {
                    city = county = country = string.Empty;
                    return false;
                }

                var countryEntity = _db.Countries.AsNoTracking().FirstOrDefault(c => c.Id == countyEntity.CountryId);
                if (countryEntity is null)
                {
                    city = county = country = string.Empty;
                    return false;
                }

                city = cityEntity.Name;
                county = countyEntity.Name;
                country = countryEntity.Name;
                return true;
            }
        }
    }
}