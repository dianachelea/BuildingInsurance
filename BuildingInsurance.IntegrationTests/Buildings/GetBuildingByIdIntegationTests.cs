using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Buildings.Queries.GetBuildingById;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Buildings;
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
using Xunit;

namespace BuildingInsurance.IntegrationTests.Buildings
{
    public sealed class GetBuildingByIdIntegationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly FakeGeographyHostedService _geography;
        private readonly GetBuildingByIdHandler _handler;

        private Guid _clientId;
        private Guid _cityId;
        private Guid _buildingId;

        public GetBuildingByIdIntegationTests()
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

            _geography = new FakeGeographyHostedService();

            _handler = new GetBuildingByIdHandler(_uow, _geography);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);

            _cityId = Guid.NewGuid();
            var city = new Domain.Entities.Geography.City(_cityId, "Cluj-Napoca", county.Id);
            _db.Cities.Add(city);

            _clientId = Guid.NewGuid();
            var client = new Domain.Entities.Clients.Client(
                id: _clientId,
                type: ClientType.Individual,
                fullName: "Test Client",
                contactInfo: new ContactInfo("client@test.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null);

            _db.Clients.Add(client);

            _buildingId = Guid.NewGuid();
            var building = new Building(
                id: _buildingId,
                clientId: _clientId,
                address: new Address("Main St", "10"),
                cityId: _cityId,
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150_000m,
                riskIndicators: RiskIndicators.None);

            _db.Buildings.Add(building);

            _db.SaveChanges();
        }

        [Fact]
        public async Task GetBuildingById_WhenBuildingDoesNotExist_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();
            var query = new GetBuildingByIdQuery(missingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Building with ID {missingId} not found.", result.Error);
        }

        [Fact]
        public async Task GetBuildingById_WhenGeographyNotFound_ShouldReturnNotFound()
        {
            _geography.ShouldSucceed = false;

            var query = new GetBuildingByIdQuery(_buildingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Geography not found for building.", result.Error);
        }

        [Fact]
        public async Task GetBuildingById_WhenBuildingAndGeographyExist_ShouldReturnDetails()
        {
            _geography.ShouldSucceed = true;
            _geography.City = "Cluj-Napoca";
            _geography.County = "Cluj";
            _geography.Country = "Romania";

            var query = new GetBuildingByIdQuery(_buildingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var dto = result.Value!;

            Assert.Equal(_buildingId, dto.Id);
            Assert.Equal(_clientId, dto.ClientId);
            Assert.Equal("MAIN ST", dto.Street);
            Assert.Equal("10", dto.Number);
            Assert.Equal("Cluj-Napoca", dto.City);
            Assert.Equal("Cluj", dto.County);
            Assert.Equal("Romania", dto.Country);
            Assert.Equal(2000, dto.ConstructionYear);
            Assert.Equal(BuildingType.Residential, dto.Type);
            Assert.Equal(2, dto.NumberOfFloors);
            Assert.Equal(120m, dto.SurfaceArea);
            Assert.Equal(150_000m, dto.InsuredValue);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        private sealed class FakeGeographyHostedService : IGeographyCachingService
        {
            public bool ShouldSucceed { get; set; }
            public string City { get; set; } = string.Empty;
            public string County { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;

            public Task LoadAsync(CancellationToken ct) => Task.CompletedTask;

            public bool TryGet(Guid cityId, out string city, out string county, out string country)
            {
                if (!ShouldSucceed)
                {
                    city = county = country = string.Empty;
                    return false;
                }

                city = City;
                county = County;
                country = Country;
                return true;
            }
        }
    }
}
