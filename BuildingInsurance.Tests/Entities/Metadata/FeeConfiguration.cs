using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Tests.Entities.Metadata
{
    public class FeeConfigurationTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenNameMissing(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: name!,
                    feeType: FeeType.BrokerCommission,
                    feePercentage: 0.1m,
                    effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    isActive: true,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("Fee name cannot be null or empty", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenFeeTypeInvalid()
        {
            var invalidType = (FeeType)999;

            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: "Invalid",
                    feeType: invalidType,
                    feePercentage: 0.1m,
                    effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    isActive: true,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("Invalid fee type", ex.Message);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(1.01)]
        [InlineData(2.0)]
        public void Constructor_ShouldThrow_WhenPercentageOutOfRange(double value)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: "Standard broker fee",
                    feeType: FeeType.BrokerCommission,
                    feePercentage: (decimal)value,
                    effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    isActive: true,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("Fee percentage must be between 0 and 1", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenEffectiveFromNotUtc()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: "Standard broker fee",
                    feeType: FeeType.BrokerCommission,
                    feePercentage: 0.1m,
                    effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                    effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    isActive: true,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("EffectiveFrom must be UTC", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenEffectiveToNotUtc()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: "Standard broker fee",
                    feeType: FeeType.BrokerCommission,
                    feePercentage: 0.1m,
                    effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Local),
                    isActive: true,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("EffectiveTo must be UTC", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenEffectiveToBeforeOrEqualFrom()
        {
            var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: "Standard broker fee",
                    feeType: FeeType.BrokerCommission,
                    feePercentage: 0.1m,
                    effectiveFrom: from,
                    effectiveTo: to,
                    isActive: true,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("EffectiveTo must be after EffectiveFrom", ex.Message);
        }

        [Fact]
        public void Constructor_RiskAdjustment_ShouldThrow_WhenRiskIndicatorsNone()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: "Earthquake risk adjustment",
                    feeType: FeeType.RiskAdjustment,
                    feePercentage: 0.1m,
                    effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    isActive: true,
                    riskIndicators: RiskIndicators.None));

            Assert.Contains("Risk fee must specify risk indicators", ex.Message);
        }

        [Fact]
        public void Constructor_NonRiskAdjustment_ShouldThrow_WhenRiskIndicatorsNotNone()
        {
            var someRisk = (RiskIndicators)1;

            var ex = Assert.Throws<ArgumentException>(() =>
                new FeeConfiguration(
                    feeName: "Admin fee",
                    feeType: FeeType.AdminFee,
                    feePercentage: 0.05m,
                    effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    isActive: true,
                    riskIndicators: someRisk));

            Assert.Contains("Only RiskAdjustment fees can have risk indicators", ex.Message);
        }

        [Fact]
        public void IsEffectiveAt_ShouldReturnFalse_WhenInactive()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: false,
                riskIndicators: RiskIndicators.None);

            var date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.False(fee.IsEffectiveAt(date));
        }

        [Fact]
        public void IsEffectiveAt_ShouldReturnFalse_WhenBeforeEffectiveFrom()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var date = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            Assert.False(fee.IsEffectiveAt(date));
        }

        [Fact]
        public void IsEffectiveAt_ShouldReturnFalse_WhenAfterEffectiveTo()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var date = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.False(fee.IsEffectiveAt(date));
        }

        [Fact]
        public void IsEffectiveAt_ShouldReturnTrue_WhenActiveAndWithinRange()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.True(fee.IsEffectiveAt(date));
        }

        [Fact]
        public void Activate_ShouldSetIsActiveTrue()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: false,
                riskIndicators: RiskIndicators.None);

            fee.Activate();

            Assert.True(fee.IsActive);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveFalse()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            fee.Deactivate();

            Assert.False(fee.IsActive);
        }

        [Fact]
        public void UpdatePercentage_ShouldThrow_WhenOutOfRange()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var ex = Assert.Throws<ArgumentException>(() => fee.UpdatePercentage(1.5m));
            Assert.Contains("Fee percentage must be between 0 and 1", ex.Message);
        }

        [Fact]
        public void UpdatePercentage_ShouldUpdateValue_WhenValid()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            fee.UpdatePercentage(0.2m);

            Assert.Equal(0.2m, fee.FeePercentage);
        }

        [Fact]
        public void UpdateRisk_ShouldThrow_WhenFeeTypeIsNotRiskAdjustment()
        {
            var fee = new FeeConfiguration(
                feeName: "Standard broker fee",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var someRisk = (RiskIndicators)1;

            var ex = Assert.Throws<InvalidOperationException>(() => fee.UpdateRisk(someRisk));
            Assert.Contains("Only RiskAdjustment fees can have risk indicators", ex.Message);
        }

        [Fact]
        public void UpdateRisk_ShouldThrow_WhenRiskIndicatorsNone_ForRiskAdjustment()
        {
            var someRisk = (RiskIndicators)1;

            var fee = new FeeConfiguration(
                feeName: "Earthquake risk adjustment",
                feeType: FeeType.RiskAdjustment,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: someRisk);

            var ex = Assert.Throws<ArgumentException>(() => fee.UpdateRisk(RiskIndicators.None));
            Assert.Contains("Risk fee must have at least one risk indicator", ex.Message);
        }

        [Fact]
        public void UpdateRisk_ShouldUpdateRiskIndicators_WhenValid()
        {
            var initialRisk = (RiskIndicators)1;
            var newRisk = (RiskIndicators)2;

            var fee = new FeeConfiguration(
                feeName: "Earthquake risk adjustment",
                feeType: FeeType.RiskAdjustment,
                feePercentage: 0.1m,
                effectiveFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: initialRisk);

            fee.UpdateRisk(newRisk);

            Assert.Equal(newRisk, fee.RiskIndicators);
        }
    }
}