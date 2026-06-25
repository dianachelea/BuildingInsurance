using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.UpdateBuilding;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
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
using BuildingInsurance.IntegrationTests.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Buildings
{
    public sealed class UpdateBuildingIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly UpdateBuildingCommandHandler _handler;

        private Guid _clientId;
        private Guid _cityId1;
        private Guid _cityId2;
        private Guid _buildingId;

        public UpdateBuildingIntegrationTests()
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

            _handler = new UpdateBuildingCommandHandler(_uow, NullLogger<UpdateBuildingCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            _db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            _db.Counties.Add(county);

            var city1 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);
            var city2 = new Domain.Entities.Geography.City(Guid.NewGuid(), "Turda", county.Id);
            _db.Cities.AddRange(city1, city2);

            var client = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Existing Client",
                contactInfo: new ContactInfo("existing@mail.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null);

            _db.Clients.Add(client);

            var building = new Building(
                id: Guid.NewGuid(),
                clientId: client.Id,
                address: new Address("Old St", "10"),
                cityId: city1.Id,
                constructionYear: 1990,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 100m,
                insuredValue: 100_000m,
                riskIndicators: RiskIndicators.None);

            _db.Buildings.Add(building);
            _db.SaveChanges();

            _clientId = client.Id;
            _cityId1 = city1.Id;
            _cityId2 = city2.Id;
            _buildingId = building.Id;
        }

        [Fact]
        public async Task UpdateBuilding_WhenNotFound_ShouldReturnNotFound()
        {
            var command = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = _cityId2,
                Address = new AddressDto { Street = "New St", Number = "99" },
                ConstructionYear = 2000,
                Type = BuildingTypeContract.Industrial,
                NumberOfFloors = 3,
                SurfaceArea = 200m,
                InsuredValue = 250_000m,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Building with ID {command.BuildingId} not found.", result.Error);
        }

        [Fact]
        public async Task UpdateBuilding_ValidInput_ShouldPersist()
        {
            var command = new UpdateBuildingCommand
            {
                BuildingId = _buildingId,
                CityId = _cityId2,
                Address = new AddressDto { Street = "New Street", Number = "22" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Industrial,
                NumberOfFloors = 5,
                SurfaceArea = 350m,
                InsuredValue = 500_000m,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal(_buildingId, result.Value!.Id);
            Assert.Equal(_clientId, result.Value.ClientId);

            var persisted = await _db.Buildings.AsNoTracking().FirstAsync(b => b.Id == _buildingId);

            Assert.Equal(_clientId, persisted.ClientId);
            Assert.Equal(_cityId2, persisted.CityId);
            Assert.Equal(2005, persisted.ConstructionYear);
            Assert.Equal(BuildingType.Industrial, persisted.Type);
            Assert.Equal(5, persisted.NumberOfFloors);
            Assert.Equal(350m, persisted.SurfaceArea);
            Assert.Equal(500_000m, persisted.InsuredValue);

            Assert.Equal("NEW STREET", persisted.Address.Street);
            Assert.Equal("22", persisted.Address.Number);
        }

        [Fact]
        public async Task UpdateBuilding_WhenCommitFails_ShouldRollback_AndNotPersistChanges()
        {
            var seedOptions = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            Guid buildingId;

            using (var seedContext = new BuildingInsuranceDbContext(seedOptions))
            {
                await seedContext.Database.EnsureCreatedAsync();

                var client = await seedContext.Clients.FirstAsync();
                var city = await seedContext.Cities.FirstAsync();

                var seededBuilding = new Building(
                    id: Guid.NewGuid(),
                    clientId: client.Id,
                    address: new Address("Rollback St", "1"),
                    cityId: city.Id,
                    constructionYear: 1999,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 80m,
                    insuredValue: 90_000m,
                    riskIndicators: RiskIndicators.None);

                seedContext.Buildings.Add(seededBuilding);
                await seedContext.SaveChangesAsync();

                buildingId = seededBuilding.Id;
            }

            var optionsFail = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(new SimulatedFailureInterceptor())
                .Options;

            using var failingContext = new BuildingInsuranceDbContext(optionsFail);

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
            var failingHandler = new UpdateBuildingCommandHandler(failingUow, NullLogger<UpdateBuildingCommandHandler>.Instance);

            var newCityId = await failingContext.Cities.Select(c => c.Id).FirstAsync();

            var command = new UpdateBuildingCommand
            {
                BuildingId = buildingId,
                CityId = newCityId,
                Address = new AddressDto { Street = "Should Not Persist", Number = "999" },
                ConstructionYear = 2020,
                Type = BuildingTypeContract.Industrial,
                NumberOfFloors = 9,
                SurfaceArea = 999m,
                InsuredValue = 999_999m,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await failingHandler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("An error occurred while updating the building.", result.Error);

            var persisted = await failingContext.Buildings.AsNoTracking().FirstAsync(b => b.Id == buildingId);

            Assert.Equal("ROLLBACK ST", persisted.Address.Street);
            Assert.Equal("1", persisted.Address.Number);
            Assert.Equal(1999, persisted.ConstructionYear);
            Assert.Equal(BuildingType.Residential, persisted.Type);
            Assert.Equal(1, persisted.NumberOfFloors);
            Assert.Equal(80m, persisted.SurfaceArea);
            Assert.Equal(90_000m, persisted.InsuredValue);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}
