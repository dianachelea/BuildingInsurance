using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Tests.Entities.Buildings
{
    public class BuildingTests
    {
        [Fact]
        public void Constructor_ShouldGenerateNewId_WhenIdIsEmpty()
        {
            var building = new Building(
                id: Guid.Empty,
                clientId: Guid.NewGuid(),
                address: new Address("Strada exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            Assert.NotEqual(Guid.Empty, building.Id);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenClientIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.Empty,
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.NewGuid(),
                    constructionYear: 2000,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("ClientId is required", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenAddressIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: null!,
                    cityId: Guid.NewGuid(),
                    constructionYear: 2000,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenCityIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.Empty,
                    constructionYear: 2000,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("CityId is required", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenConstructionYearTooSmall()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.NewGuid(),
                    constructionYear: 1799,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("ConstructionYear must be between 1800", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenConstructionYearInFuture()
        {
            var future = DateTime.UtcNow.Year + 1;

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.NewGuid(),
                    constructionYear: future,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("ConstructionYear must be between 1800", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_ShouldThrow_WhenNumberOfFloorsNotGreaterThanZero(int floors)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.NewGuid(),
                    constructionYear: 2000,
                    type: BuildingType.Residential,
                    numberOfFloors: floors,
                    surfaceArea: 100m,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("NumberOfFloors must be greater than 0", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Constructor_ShouldThrow_WhenSurfaceAreaNotGreaterThanZero(decimal area)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.NewGuid(),
                    constructionYear: 2000,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: area,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("SurfaceArea must be greater than 0", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Constructor_ShouldThrow_WhenInsuredValueNotGreaterThanZero(decimal insuredValue)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.NewGuid(),
                    constructionYear: 2000,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: insuredValue,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("InsuredValue must be greater than 0", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenBuildingTypeInvalid()
        {
            var invalidType = (BuildingType)999;

            var ex = Assert.Throws<ArgumentException>(() =>
                new Building(
                    id: Guid.NewGuid(),
                    clientId: Guid.NewGuid(),
                    address: new Address("Strada exemplu", "10A"),
                    cityId: Guid.NewGuid(),
                    constructionYear: 2000,
                    type: invalidType,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: 50000m,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("Invalid building type", ex.Message);
        }

        [Fact]
        public void ChangeAddress_ShouldThrow_WhenAddressIsNull()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            Assert.Throws<ArgumentNullException>(() => building.ChangeAddress(null!));
        }

        [Fact]
        public void ChangeAddress_ShouldUpdate_Address_Only()
        {
            var initialAddress = new Address("Strada exemplu", "10A");
            var initialCityId = Guid.NewGuid();

            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: initialAddress,
                cityId: initialCityId,
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            var newAddress = new Address("Strada noua", "99");

            building.ChangeAddress(newAddress);

            Assert.Equal(newAddress, building.Address);
            Assert.Equal(initialCityId, building.CityId);
        }

        [Fact]
        public void Relocate_ShouldThrow_WhenAddressIsNull()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);


            Assert.Throws<ArgumentNullException>(() => building.Relocate(null!, Guid.NewGuid()));
        }

        [Fact]
        public void Relocate_ShouldThrow_WhenCityIdIsEmpty()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            var ex = Assert.Throws<ArgumentException>(() =>
                building.Relocate(new Address("Strada noua", "99"), Guid.Empty));

            Assert.Contains("CityId is required", ex.Message);
        }

        [Fact]
        public void Relocate_ShouldUpdate_Address_And_CityId()
        {
            var initialAddress = new Address("Strada exemplu", "10A");
            var initialCityId = Guid.NewGuid();

            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: initialAddress,
                cityId: initialCityId,
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            var newAddress = new Address("Strada noua", "99");
            var newCityId = Guid.NewGuid();

            building.Relocate(newAddress, newCityId);

            Assert.Equal(newAddress, building.Address);
            Assert.Equal(newCityId, building.CityId);
            Assert.NotEqual(initialAddress, building.Address);
            Assert.NotEqual(initialCityId, building.CityId);
        }

        [Fact]
        public void UpdateInsuredValue_ShouldThrow_WhenInsuredValueInvalid()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => building.UpdateInsuredValue(0));

            Assert.Contains("InsuredValue must be greater than 0", ex.Message);
        }

        [Fact]
        public void UpdateConstruction_ShouldUpdateFields()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            building.UpdateConstruction(
                constructionYear: 1999,
                type: BuildingType.Office,
                floors: 3,
                surfaceArea: 250m);

            Assert.Equal(1999, building.ConstructionYear);
            Assert.Equal(BuildingType.Office, building.Type);
            Assert.Equal(3, building.NumberOfFloors);
            Assert.Equal(250m, building.SurfaceArea);
        }

        [Fact]
        public void UpdateRiskIndicators_ShouldUpdateRiskIndicators()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            var newRisks = RiskIndicators.FloodZone | RiskIndicators.EarthquakeProne;

            building.UpdateRiskIndicators(newRisks);

            Assert.Equal(newRisks, building.RiskIndicators);
        }

        [Fact]
        public void AddRisk_ShouldSetFlag()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada Exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Office,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.None);

            building.AddRisk(RiskIndicators.FloodZone);

            Assert.True(building.RiskIndicators.HasFlag(RiskIndicators.FloodZone));
        }

        [Fact]
        public void RemoveRisk_ShouldUnsetFlag()
        {
            var building = new Building(
                id: Guid.NewGuid(),
                clientId: Guid.NewGuid(),
                address: new Address("Strada Exemplu", "10A"),
                cityId: Guid.NewGuid(),
                constructionYear: 2000,
                type: BuildingType.Residential,
                numberOfFloors: 1,
                surfaceArea: 100m,
                insuredValue: 50000m,
                riskIndicators: RiskIndicators.EarthquakeProne | RiskIndicators.FloodZone);

            building.RemoveRisk(RiskIndicators.EarthquakeProne);

            Assert.False(building.RiskIndicators.HasFlag(RiskIndicators.EarthquakeProne));
            Assert.True(building.RiskIndicators.HasFlag(RiskIndicators.FloodZone));
        }
    }
}