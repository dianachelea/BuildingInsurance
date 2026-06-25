using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Services;
using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Selection;
using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Geography;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Moq;

namespace BuildingInsurance.Tests.Services.Policy
{
    public sealed class PolicyPricingServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IBuildingRepository> _buildings = new();
        private readonly Mock<ICityRepository> _cities = new();
        private readonly Mock<ICountyRepository> _counties = new();
        private readonly Mock<IFeeConfigurationRepository> _fees = new();
        private readonly Mock<IRiskFactorConfigurationRepository> _risks = new();

        private readonly PolicyPricingService _service;

        public PolicyPricingServiceTests()
        {
            _uow.SetupGet(x => x.Buildings).Returns(_buildings.Object);
            _uow.SetupGet(x => x.Cities).Returns(_cities.Object);
            _uow.SetupGet(x => x.Counties).Returns(_counties.Object);
            _uow.SetupGet(x => x.FeeConfigurations).Returns(_fees.Object);
            _uow.SetupGet(x => x.RiskFactorConfigurations).Returns(_risks.Object);

            var draftStrategy = new DraftPolicyPricingStrategy(_uow.Object);
            var snapshotStrategy = new SnapshotPolicyPricingStrategy();

            var selector = new PolicyPricingStrategySelector(new IPolicyPricingStrategy[]
            {
                draftStrategy,
                snapshotStrategy
            });

            _service = new PolicyPricingService(selector);
        }

        [Fact]
        public async Task CalculateDraftAsync_ShouldReturn_BasePremium_When_No_Fees_And_No_Risks()
        {
            var buildingId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var countyId = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var effectiveAtUtc = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
            var basePremium = 100m;

            var building = new Building(
                id: buildingId,
                clientId: Guid.NewGuid(),
                address: new Address("Main Street", "10"),
                cityId: cityId,
                constructionYear: 2005,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150000m,
                riskIndicators: RiskIndicators.None);

            _buildings.Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            var city = new City(cityId, "Bucharest", countyId);
            _cities.Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(city);

            var county = new County(countyId, "Ilfov", countryId);
            _counties.Setup(r => r.GetByIdAsync(countyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(county);

            _fees.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FeeConfiguration>());

            _risks.Setup(r => r.GetActiveForAsync(countryId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());
            _risks.Setup(r => r.GetActiveForAsync(countyId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());
            _risks.Setup(r => r.GetActiveForAsync(cityId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());
            _risks.Setup(r => r.GetActiveForAsync(null, BuildingType.Residential, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: building.ClientId,
                buildingId: building.Id,
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: effectiveAtUtc,
                endDate: effectiveAtUtc.AddYears(1),
                basePremium: basePremium);

            var result = await _service.CalculateAsync(policy, building, CancellationToken.None);

            Assert.Equal(100m, result.FinalPremium);
            Assert.Empty(result.Fees);
            Assert.Empty(result.Risks);
        }

        [Fact]
        public async Task CalculateDraftAsync_ShouldIgnore_Inactive_And_OutOfDate_Fees()
        {
            var buildingId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var countyId = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var effectiveAtUtc = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
            var basePremium = 100m;

            var building = new Building(
                id: buildingId,
                clientId: Guid.NewGuid(),
                address: new Address("Main Street", "10"),
                cityId: cityId,
                constructionYear: 2005,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150000m,
                riskIndicators: RiskIndicators.None);

            _buildings.Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            _cities.Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new City(cityId, "Bucharest", countyId));

            _counties.Setup(r => r.GetByIdAsync(countyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new County(countyId, "Ilfov", countryId));

            var activeInRange = new FeeConfiguration(
                feeName: "Admin fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var inactive = new FeeConfiguration(
                feeName: "Inactive fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.20m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: false,
                riskIndicators: RiskIndicators.None);

            var outOfDate = new FeeConfiguration(
                feeName: "Old fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.30m,
                effectiveFrom: new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            _fees.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FeeConfiguration> { activeInRange, inactive, outOfDate });

            _risks.Setup(r => r.GetActiveForAsync(It.IsAny<Guid>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());
            _risks.Setup(r => r.GetActiveForAsync(null, BuildingType.Residential, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: building.ClientId,
                buildingId: building.Id,
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: effectiveAtUtc,
                endDate: effectiveAtUtc.AddYears(1),
                basePremium: basePremium);

            var result = await _service.CalculateAsync(policy, building, CancellationToken.None);

            Assert.Equal(110m, result.FinalPremium);
            Assert.Single(result.Fees);
            Assert.Empty(result.Risks);
        }

        [Fact]
        public async Task CalculateDraftAsync_ShouldApply_RiskAdjustmentFee_Only_When_Building_Has_Indicators()
        {
            var buildingId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var countyId = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var effectiveAtUtc = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
            var basePremium = 100m;

            var building = new Building(
                id: buildingId,
                clientId: Guid.NewGuid(),
                address: new Address("Main Street", "10"),
                cityId: cityId,
                constructionYear: 2005,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150000m,
                riskIndicators: RiskIndicators.FloodZone);

            _buildings.Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            _cities.Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new City(cityId, "Bucharest", countyId));

            _counties.Setup(r => r.GetByIdAsync(countyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new County(countyId, "Ilfov", countryId));

            var floodFee = new FeeConfiguration(
                feeName: "Flood fee",
                feeType: FeeType.RiskAdjustment,
                feePercentage: 0.15m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.FloodZone);

            var quakeFee = new FeeConfiguration(
                feeName: "Earthquake fee",
                feeType: FeeType.RiskAdjustment,
                feePercentage: 0.20m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.EarthquakeProne);

            _fees.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FeeConfiguration> { floodFee, quakeFee });

            _risks.Setup(r => r.GetActiveForAsync(It.IsAny<Guid>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());
            _risks.Setup(r => r.GetActiveForAsync(null, BuildingType.Residential, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: building.ClientId,
                buildingId: building.Id,
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: effectiveAtUtc,
                endDate: effectiveAtUtc.AddYears(1),
                basePremium: basePremium);

            var result = await _service.CalculateAsync(policy, building, CancellationToken.None);

            Assert.Equal(115m, result.FinalPremium);
            Assert.Single(result.Fees);
            Assert.Equal("Flood fee", result.Fees[0].FeeName);
        }

        [Fact]
        public async Task CalculateDraftAsync_ShouldApply_Fees_And_Risks_Using_Correct_Formula()
        {
            var buildingId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var countyId = Guid.NewGuid();
            var countryId = Guid.NewGuid();

            var effectiveAtUtc = new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc);
            var basePremium = 100m;

            var building = new Building(
                id: buildingId,
                clientId: Guid.NewGuid(),
                address: new Address("Main Street", "10"),
                cityId: cityId,
                constructionYear: 2005,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150000m,
                riskIndicators: RiskIndicators.None);

            _buildings.Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            _cities.Setup(r => r.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new City(cityId, "Bucharest", countyId));

            _counties.Setup(r => r.GetByIdAsync(countyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new County(countyId, "Ilfov", countryId));

            var fee1 = new FeeConfiguration("Admin", FeeType.AdminFee, 0.10m,
                new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                true, RiskIndicators.None);

            var fee2 = new FeeConfiguration("Service", FeeType.AdminFee, 0.05m,
                new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                true, RiskIndicators.None);

            _fees.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FeeConfiguration> { fee1, fee2 });

            var riskCountry = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: countryId,
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            _risks.Setup(r => r.GetActiveForAsync(countryId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration> { riskCountry });

            _risks.Setup(r => r.GetActiveForAsync(countyId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());
            _risks.Setup(r => r.GetActiveForAsync(cityId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());
            _risks.Setup(r => r.GetActiveForAsync(null, BuildingType.Residential, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RiskFactorConfiguration>());

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: building.ClientId,
                buildingId: building.Id,
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: effectiveAtUtc,
                endDate: effectiveAtUtc.AddYears(1),
                basePremium: basePremium);

            var result = await _service.CalculateAsync(policy, building, CancellationToken.None);

            Assert.Equal(125m, result.FinalPremium);
            Assert.Equal(2, result.Fees.Count);
            Assert.Single(result.Risks);
        }
    }
}