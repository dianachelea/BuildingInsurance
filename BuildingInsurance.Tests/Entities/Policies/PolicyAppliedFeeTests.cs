using BuildingInsurance.Domain.Entities.Policies;

namespace BuildingInsurance.Tests.Entities.Policies
{
    public class PolicyAppliedFeeTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenPolicyIdEmpty()
        {
            Assert.Throws<ArgumentException>(() => new PolicyAppliedFee(Guid.Empty, Guid.NewGuid(), "Fee", 0.1m, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenFeeConfigurationIdEmpty()
        {
            Assert.Throws<ArgumentException>(() => new PolicyAppliedFee(Guid.NewGuid(), Guid.Empty, "Fee", 0.1m, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenFeeNameMissing(string? name)
        {
            Assert.Throws<ArgumentException>(() => new PolicyAppliedFee(Guid.NewGuid(), Guid.NewGuid(), name!, 0.1m, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(1.01)]
        [InlineData(2.0)]
        public void Constructor_ShouldThrow_WhenPercentageOutOfRange(double value)
        {
            var percentage = (decimal)value;
            Assert.Throws<ArgumentOutOfRangeException>(() => new PolicyAppliedFee(Guid.NewGuid(), Guid.NewGuid(), "Fee", percentage, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenAppliedAtNotUtc()
        {
            var local = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.Throws<ArgumentException>(() => new PolicyAppliedFee(Guid.NewGuid(), Guid.NewGuid(), "Fee", 0.1m, local));
        }

        [Fact]
        public void Constructor_ShouldSetFields_WhenValid()
        {
            var policyId = Guid.NewGuid();
            var feeConfigId = Guid.NewGuid();
            var appliedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var paf = new PolicyAppliedFee(policyId, feeConfigId, " Broker fee ", 0.15m, appliedAt);

            Assert.NotEqual(Guid.Empty, paf.Id);
            Assert.Equal(policyId, paf.PolicyId);
            Assert.Equal(feeConfigId, paf.FeeConfigurationId);
            Assert.Equal("Broker fee", paf.FeeName);
            Assert.Equal(0.15m, paf.Percentage);
            Assert.Equal(appliedAt, paf.AppliedAtUtc);
        }
    }
}