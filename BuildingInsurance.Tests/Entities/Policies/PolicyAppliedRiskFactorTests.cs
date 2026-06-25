using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Tests.Entities.Policies
{
    public class PolicyAppliedRiskFactorTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenPolicyIdEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                new PolicyAppliedRiskFactor(
                    Guid.Empty,
                    Guid.NewGuid(),
                    RiskFactorLevel.City,
                    Guid.NewGuid(),
                    null,
                    0.1m,
                    new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenRiskFactorConfigurationIdEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
                new PolicyAppliedRiskFactor(
                    Guid.NewGuid(),
                    Guid.Empty,
                    RiskFactorLevel.City,
                    Guid.NewGuid(),
                    null,
                    0.1m,
                    new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenLevelInvalid()
        {
            var invalidLevel = (RiskFactorLevel)999;

            Assert.Throws<ArgumentException>(() =>
                new PolicyAppliedRiskFactor(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    invalidLevel,
                    Guid.NewGuid(),
                    null,
                    0.1m,
                    new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenBuildingTypeLevel_AndBuildingTypeMissing()
        {
            Assert.Throws<ArgumentException>(() =>
                new PolicyAppliedRiskFactor(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    RiskFactorLevel.BuildingType,
                    referenceId: Guid.NewGuid(),
                    buildingType: null,
                    adjustmentPercentage: 0.1m,
                    appliedAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenGeographicLevel_AndReferenceIdMissing()
        {
            Assert.Throws<ArgumentException>(() =>
                new PolicyAppliedRiskFactor(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    RiskFactorLevel.City,
                    referenceId: null,
                    buildingType: BuildingType.Industrial,
                    adjustmentPercentage: 0.1m,
                    appliedAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenAppliedAtNotUtc()
        {
            var local = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

            Assert.Throws<ArgumentException>(() =>
                new PolicyAppliedRiskFactor(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    RiskFactorLevel.City,
                    Guid.NewGuid(),
                    null,
                    0.1m,
                    local));
        }

        [Fact]
        public void Constructor_ShouldSetFields_WhenValid_Geographic()
        {
            var policyId = Guid.NewGuid();
            var riskConfigId = Guid.NewGuid();
            var referenceId = Guid.NewGuid();
            var appliedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var prf = new PolicyAppliedRiskFactor(
                policyId,
                riskConfigId,
                RiskFactorLevel.City,
                referenceId,
                buildingType: BuildingType.Residential,
                adjustmentPercentage: 0.15m,
                appliedAtUtc: appliedAt);

            Assert.NotEqual(Guid.Empty, prf.Id);
            Assert.Equal(policyId, prf.PolicyId);
            Assert.Equal(riskConfigId, prf.RiskFactorConfigurationId);
            Assert.Equal(RiskFactorLevel.City, prf.Level);
            Assert.Equal(referenceId, prf.ReferenceId);
            Assert.Null(prf.BuildingType);
            Assert.Equal(0.15m, prf.AdjustmentPercentage);
            Assert.Equal(appliedAt, prf.AppliedAtUtc);
        }

        [Fact]
        public void Constructor_ShouldSetFields_WhenValid_BuildingType()
        {
            var policyId = Guid.NewGuid();
            var riskConfigId = Guid.NewGuid();
            var appliedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var prf = new PolicyAppliedRiskFactor(
                policyId,
                riskConfigId,
                RiskFactorLevel.BuildingType,
                referenceId: Guid.NewGuid(),
                buildingType: BuildingType.Industrial,
                adjustmentPercentage: 0.2m,
                appliedAtUtc: appliedAt);

            Assert.NotEqual(Guid.Empty, prf.Id);
            Assert.Equal(policyId, prf.PolicyId);
            Assert.Equal(riskConfigId, prf.RiskFactorConfigurationId);
            Assert.Equal(RiskFactorLevel.BuildingType, prf.Level);
            Assert.Null(prf.ReferenceId);
            Assert.Equal(BuildingType.Industrial, prf.BuildingType);
            Assert.Equal(0.2m, prf.AdjustmentPercentage);
            Assert.Equal(appliedAt, prf.AppliedAtUtc);
        }

        [Theory]
        [InlineData(-0.99)]
        [InlineData(-0.01)]
        [InlineData(0)]
        [InlineData(0.5)]
        [InlineData(0.99)]
        public void Constructor_ShouldNotThrow_WhenAdjustmentPercentageIsInRange(double value)
        {
            var percentage = (decimal)value;

            var exception = Record.Exception(() =>
                new PolicyAppliedRiskFactor(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    RiskFactorLevel.City,
                    Guid.NewGuid(),
                    null,
                    percentage,
                    new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1.01)]
        [InlineData(1)]
        [InlineData(1.1)]
        public void Constructor_ShouldThrow_WhenAdjustmentPercentageOutOfRange(double value)
        {
            var percentage = (decimal)value;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PolicyAppliedRiskFactor(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    RiskFactorLevel.City,
                    Guid.NewGuid(),
                    null,
                    percentage,
                    new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }
    }
}