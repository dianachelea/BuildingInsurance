using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Tests.Entities.Metadata
{
    public class RiskFactorConfigurationTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenLevelInvalid()
        {
            var invalidLevel = (RiskFactorLevel)999;

            var ex = Assert.Throws<ArgumentException>(() =>
                new RiskFactorConfiguration(
                    level: invalidLevel,
                    referenceId: Guid.NewGuid(),
                    buildingType: null,
                    adjustmentPercentage: 0.1m,
                    isActive: true));

            Assert.Contains("Invalid risk factor level", ex.Message);
        }

        [Fact]
        public void Constructor_BuildingTypeLevel_ShouldThrow_WhenBuildingTypeNull()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new RiskFactorConfiguration(
                    level: RiskFactorLevel.BuildingType,
                    referenceId: null,
                    buildingType: null,
                    adjustmentPercentage: 0.1m,
                    isActive: true));

            Assert.Contains("BuildingType is required when Level is BuildingType", ex.Message);
        }

        [Fact]
        public void Constructor_BuildingTypeLevel_ShouldSetBuildingType_AndClearReferenceId()
        {
            var someBuildingType = (BuildingType)1;

            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.BuildingType,
                referenceId: null,
                buildingType: someBuildingType,
                adjustmentPercentage: 0.1m,
                isActive: true);

            Assert.Equal(RiskFactorLevel.BuildingType, rfc.Level);
            Assert.Equal(someBuildingType, rfc.BuildingType);
            Assert.Null(rfc.ReferenceId);
        }

        [Fact]
        public void Constructor_GeographicLevel_ShouldThrow_WhenReferenceIdMissing()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new RiskFactorConfiguration(
                    level: RiskFactorLevel.City,
                    referenceId: null,
                    buildingType: null,
                    adjustmentPercentage: 0.1m,
                    isActive: true));

            Assert.Contains("ReferenceId is required for geographic levels", ex.Message);
        }

        [Fact]
        public void Constructor_GeographicLevel_ShouldSetReferenceId_AndClearBuildingType()
        {
            var refId = Guid.NewGuid();

            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: refId,
                buildingType: null,
                adjustmentPercentage: 0.1m,
                isActive: true);

            Assert.Equal(RiskFactorLevel.Country, rfc.Level);
            Assert.Equal(refId, rfc.ReferenceId);
            Assert.Null(rfc.BuildingType);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(1.0)]
        [InlineData(1.5)]
        public void Constructor_ShouldThrow_WhenAdjustmentPercentageOutOfRange(double value)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RiskFactorConfiguration(
                    level: RiskFactorLevel.Country,
                    referenceId: Guid.NewGuid(),
                    buildingType: null,
                    adjustmentPercentage: (decimal)value,
                    isActive: true));

            Assert.Contains("between -1 and 1", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldGenerateId()
        {
            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.2m,
                isActive: true);

            Assert.NotEqual(Guid.Empty, rfc.Id);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveTrue()
        {
            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.2m,
                isActive: false);

            rfc.Activate();

            Assert.True(rfc.IsActive);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveFalse()
        {
            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.2m,
                isActive: true);

            rfc.Deactivate();

            Assert.False(rfc.IsActive);
        }

        [Fact]
        public void UpdateAdjustmentPercentage_ShouldThrow_WhenOutOfRange()
        {
            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.2m,
                isActive: true);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => rfc.UpdateAdjustmentPercentage(1.0m));
            Assert.Contains("AdjustmentPercentage must be between -1 and 1 (exclusive)", ex.Message);
        }

        [Fact]
        public void UpdateAdjustmentPercentage_ShouldUpdateValue_WhenValid()
        {
            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.2m,
                isActive: true);

            rfc.UpdateAdjustmentPercentage(0.3m);

            Assert.Equal(0.3m, rfc.AdjustmentPercentage);
        }

        [Fact]
        public void UpdateTarget_ShouldUpdateToBuildingTypeLevel()
        {
            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.Country,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.2m,
                isActive: true);

            var buildingType = (BuildingType)1;

            rfc.UpdateTarget(
                level: RiskFactorLevel.BuildingType,
                referenceId: null,
                buildingType: buildingType);

            Assert.Equal(RiskFactorLevel.BuildingType, rfc.Level);
            Assert.Equal(buildingType, rfc.BuildingType);
            Assert.Null(rfc.ReferenceId);
        }

        [Fact]
        public void UpdateTarget_ShouldUpdateToGeographicLevel()
        {
            var buildingType = (BuildingType)1;

            var rfc = new RiskFactorConfiguration(
                level: RiskFactorLevel.BuildingType,
                referenceId: null,
                buildingType: buildingType,
                adjustmentPercentage: 0.2m,
                isActive: true);

            var refId = Guid.NewGuid();

            rfc.UpdateTarget(
                level: RiskFactorLevel.City,
                referenceId: refId,
                buildingType: null);

            Assert.Equal(RiskFactorLevel.City, rfc.Level);
            Assert.Equal(refId, rfc.ReferenceId);
            Assert.Null(rfc.BuildingType);
        }
    }
}