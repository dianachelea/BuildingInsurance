using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.CreateBuilding;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Mapping;
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
    public sealed class CreateBuildingIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly CreateBuildingCommandHandler _handler;

        public CreateBuildingIntegrationTests()
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

            _handler = new CreateBuildingCommandHandler(_uow, NullLogger<CreateBuildingCommandHandler>.Instance);

            SeedTestData();
        }

        private void SeedTestData()
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
                fullName: "Existing Client",
                contactInfo: new ContactInfo("existing@mail.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null);

            _db.Clients.Add(client);

            _db.SaveChanges();
        }

        [Fact]
        public async Task CreateBuildingValidInput_ShouldPersist()
        {
            var client = await _db.Clients.AsNoTracking().FirstAsync();
            var city = await _db.Cities.AsNoTracking().FirstAsync();

            var command = new CreateBuildingCommand
            {
                ClientId = client.Id,
                CityId = city.Id,
                Street = "Main St",
                Number = "10",
                ConstructionYear = 2000,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 120m,
                InsuredValue = 150_000m,
                RiskIndicators = new RiskIndicatorsContract()
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);

            var persisted = await _db.Buildings
                .AsNoTracking()
                .Include(b => b.Address)
                .FirstOrDefaultAsync(b => b.Id == result.Value.Id);

            Assert.NotNull(persisted);
            Assert.Equal(command.ClientId, persisted!.ClientId);
            Assert.Equal("MAIN ST", persisted.Address.Street);
            Assert.Equal("10", persisted.Address.Number);
            Assert.Equal(command.CityId, persisted.CityId);
            Assert.Equal(command.ConstructionYear, persisted.ConstructionYear);
            Assert.Equal(command.Type.MapToDomainBuildingType(), persisted.Type);
            Assert.Equal(command.NumberOfFloors, persisted.NumberOfFloors);
            Assert.Equal(command.SurfaceArea, persisted.SurfaceArea);
            Assert.Equal(command.InsuredValue, persisted.InsuredValue);
        }

        [Fact]
        public async Task CreateBuildingClientNotFound_ShouldReturnNotFound()
        {
            var city = await _db.Cities.AsNoTracking().FirstAsync();

            var command = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = city.Id,
                Street = "Main St",
                Number = "11",
                ConstructionYear = 2001,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 1,
                SurfaceArea = 80m,
                InsuredValue = 90_000m,
                RiskIndicators = new RiskIndicatorsContract()
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Client with ID {command.ClientId} not found.", result.Error);

            var exists = await _db.Buildings.AsNoTracking().AnyAsync(b => b.Address.Street == "MAIN ST" && b.Address.Number == "11");
            Assert.False(exists);
        }

        [Fact]
        public async Task CreateBuildingCityNotFound_ShouldReturnNotFound()
        {
            var client = await _db.Clients.AsNoTracking().FirstAsync();

            var command = new CreateBuildingCommand
            {
                ClientId = client.Id,
                CityId = Guid.NewGuid(),
                Street = "Main St",
                Number = "12",
                ConstructionYear = 2002,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 1,
                SurfaceArea = 80m,
                InsuredValue = 90_000m,
                RiskIndicators = new RiskIndicatorsContract()
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"City with ID {command.CityId} not found.", result.Error);

            var exists = await _db.Buildings.AsNoTracking().AnyAsync(b => b.Address.Street == "MAIN ST" && b.Address.Number == "12");
            Assert.False(exists);
        }

        [Fact]
        public async Task CreateBuildingAddressExists_ShouldReturnConflict_AndNotPersist()
        {
            var client = await _db.Clients.FirstAsync();
            var city = await _db.Cities.FirstAsync();

            var existing = new Building(
                id: Guid.NewGuid(),
                clientId: client.Id,
                address: new Address("Dup St", "5"),
                cityId: city.Id,
                constructionYear: 1995,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 50m,
                insuredValue: 50_000m,
                riskIndicators: RiskIndicators.None);

            _db.Buildings.Add(existing);
            await _db.SaveChangesAsync();

            var command = new CreateBuildingCommand
            {
                ClientId = client.Id,
                CityId = city.Id,
                Street = "Dup St",
                Number = "5",
                ConstructionYear = 2020,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 200m,
                InsuredValue = 250_000m,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Building already exists at the specified address for this client.", result.Error);

            var exists = await _db.Buildings.AsNoTracking().CountAsync(b => b.ClientId == client.Id && b.Address.Street == "DUP ST" && b.Address.Number == "5");
            Assert.Equal(1, exists);
        }

        [Fact]
        public async Task CreateBuildingWhenCommitFails_ShouldRollback()
        {
            var client = await _db.Clients.AsNoTracking().FirstAsync();
            var city = await _db.Cities.AsNoTracking().FirstAsync();

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
            var failingHandler = new CreateBuildingCommandHandler(failingUow, NullLogger<CreateBuildingCommandHandler>.Instance);

            var command = new CreateBuildingCommand
            {
                ClientId = client.Id,
                CityId = city.Id,
                Street = "Rollback St",
                Number = "99",
                ConstructionYear = 2021,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 1,
                SurfaceArea = 60m,
                InsuredValue = 70_000m,
                RiskIndicators = new RiskIndicatorsContract()
            };

            var result = await failingHandler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during buildind creation.", result.Error);

            var exists = await failingContext.Buildings.AsNoTracking()
                .AnyAsync(b => b.ClientId == client.Id && b.Address.Street == "ROLLBACK ST" && b.Address.Number == "99");

            Assert.False(exists, "Building should not exist in DB because transaction should have rolled back.");
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}