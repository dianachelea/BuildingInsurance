using BuildingInsurance.Domain.Constants;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Tests.Entities.Policies
{
    public class PolicyTests
    {
        [Fact]
        public void CreateDraft_ShouldThrow_WhenClientIdEmpty()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentException>(() =>
                Policy.CreateDraft(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m));

            Assert.Contains("ClientId is required.", ex.Message);
        }

        [Fact]
        public void CreateDraft_ShouldThrow_WhenBuildingIdEmpty()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentException>(() =>
                Policy.CreateDraft(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), start, end, 100m));

            Assert.Contains("BuildingId is required.", ex.Message);
        }

        [Fact]
        public void CreateDraft_ShouldThrow_WhenBrokerIdEmpty()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentException>(() =>
                Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), start, end, 100m));

            Assert.Contains("BrokerId is required.", ex.Message);
        }

        [Fact]
        public void CreateDraft_ShouldThrow_WhenCurrencyIdEmpty()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentException>(() =>
                Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, start, end, 100m));

            Assert.Contains("CurrencyId is required.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void CreateDraft_ShouldThrow_WhenBasePremiumNotPositive(decimal basePremium)
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, basePremium));

            Assert.Contains("Base premium must be positive.", ex.Message);
        }

        [Fact]
        public void CreateDraft_ShouldThrow_WhenDatesNotUtc()
        {
            var nonUtcStart = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Local);
            var utcEnd = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentException>(() =>
                Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), nonUtcStart, utcEnd, 100m));

            Assert.Contains("Dates must be in UTC.", ex.Message);
        }

        [Fact]
        public void CreateDraft_ShouldThrow_WhenEndDate_NotAfterStartDate()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var endBefore = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<ArgumentException>(() =>
                Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, endBefore, 100m));

            Assert.Contains("EndDate must be after StartDate.", ex.Message);
        }

        [Fact]
        public void CreateDraft_ShouldInitializeFields_Correctly()
        {
            var clientId = Guid.NewGuid();
            var buildingId = Guid.NewGuid();
            var brokerId = Guid.NewGuid();
            var currencyId = Guid.NewGuid();
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var policy = Policy.CreateDraft(clientId, buildingId, brokerId, currencyId, start, end, 1234.56m);

            Assert.Equal(clientId, policy.ClientId);
            Assert.Equal(buildingId, policy.BuildingId);
            Assert.Equal(brokerId, policy.BrokerId);
            Assert.Equal(currencyId, policy.CurrencyId);
            Assert.Equal(start, policy.StartDate);
            Assert.Equal(end, policy.EndDate);
            Assert.Equal(1234.56m, policy.BasePremium);
            Assert.Equal(0m, policy.FinalPremium);
            Assert.Equal(PolicyStatus.Draft, policy.PolicyStatus);

            Assert.StartsWith("POL-", policy.PolicyNumber);
            Assert.Equal(16, policy.PolicyNumber.Length);
        }

        [Fact]
        public void Activate_ShouldThrow_WhenStartDateInPast()
        {
            var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            var now = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<InvalidOperationException>(() => policy.Activate(now));
            Assert.Contains("Start date should not be in the past.", ex.Message);
        }

        [Fact]
        public void Activate_ShouldThrow_WhenNotDraft()
        {
            var start = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            policy.SetPricing(
                finalPremium: 1234m,
                appliedFees: Array.Empty<PolicyAppliedFee>(),
                appliedRiskFactors: Array.Empty<PolicyAppliedRiskFactor>());

            policy.SetFinalPremiumInBaseCurrency(1234m);

            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            policy.Activate(now);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                policy.Activate(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)));

            Assert.Contains("Only Draft policies can be activated.", ex.Message);
        }

        [Fact]
        public void Activate_ShouldThrow_WhenNowIsNotUtc()
        {
            var start = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            var nonUtcNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

            var ex = Assert.Throws<ArgumentException>(() => policy.Activate(nonUtcNow));
            Assert.Contains("nowUtc must be UTC.", ex.Message);
        }

        [Fact]
        public void Activate_ShouldSetStatusToActive_WhenValid()
        {
            var start = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            policy.SetPricing(
                finalPremium: 1234m,
                appliedFees: Array.Empty<PolicyAppliedFee>(),
                appliedRiskFactors: Array.Empty<PolicyAppliedRiskFactor>());

            policy.SetFinalPremiumInBaseCurrency(1234m);

            policy.Activate(now);

            Assert.Equal(PolicyStatus.Active, policy.PolicyStatus);
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenNotActive()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                policy.Cancel("ANY", new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)));

            Assert.Contains("Only Active policies can be cancelled.", ex.Message);
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenCancellationDateNotUtc()
        {
            var start = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            policy.SetPricing(1234m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(1234m);
            policy.Activate(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var nonUtcCancellation = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Local);

            var ex = Assert.Throws<ArgumentException>(() => policy.Cancel("ANY", nonUtcCancellation));
            Assert.Contains("Dates must be UTC.", ex.Message);
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenCancellationDateBeforeStartDate()
        {
            var start = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            policy.SetPricing(1234m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(1234m);
            policy.Activate(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var cancellationDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<InvalidOperationException>(() => policy.Cancel("ANY", cancellationDate));
            Assert.Contains("Cancellation effective date cannot be before StartDate.", ex.Message);
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenCancellationDateAfterEndDate()
        {
            var start = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            policy.SetPricing(1234m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(1234m);
            policy.Activate(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var cancellationDate = new DateTime(2027, 6, 11, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<InvalidOperationException>(() => policy.Cancel("ANY", cancellationDate));
            Assert.Contains("Cancellation effective date cannot be after EndDate.", ex.Message);
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenReasonInvalid()
        {
            var start = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            policy.SetPricing(1234m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(1234m);
            policy.Activate(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var cancellationDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                policy.Cancel("THIS_IS_NOT_ALLOWED", cancellationDate));

            Assert.Contains("Invalid cancellation reason.", ex.Message);
        }

        [Fact]
        public void Cancel_ShouldSetStatusToCancelled_WhenValid()
        {
            var start = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 10, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            policy.SetPricing(1234m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(1234m);
            policy.Activate(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var cancellationDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);

            var validReason = CancellationReasons.Allowed[0];
            policy.Cancel(validReason, cancellationDate);

            Assert.Equal(PolicyStatus.Cancelled, policy.PolicyStatus);
            Assert.Equal(cancellationDate, policy.CancellationEffectiveDate);
        }

        [Fact]
        public void SetPricing_ShouldThrow_WhenNotDraft()
        {
            var start = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            policy.SetPricing(1234m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(1234m);
            policy.Activate(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var risk = new PolicyAppliedRiskFactor(
                policyId: policy.Id,
                riskFactorConfigurationId: Guid.NewGuid(),
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.1m,
                appliedAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var fee = new PolicyAppliedFee(
                policyId: policy.Id,
                feeConfigurationId: Guid.NewGuid(),
                feeName: "F",
                percentage: 0.1m,
                appliedAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var ex = Assert.Throws<InvalidOperationException>(() => policy.SetPricing(110m, new[] { fee }, new[] { risk }));
            Assert.Contains("Pricing can be set only for Draft policies.", ex.Message);
        }

        [Fact]
        public void SetPricing_ShouldThrow_WhenFinalPremiumNotPositive()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            var fee = new PolicyAppliedFee(
                policyId: policy.Id,
                feeConfigurationId: Guid.NewGuid(),
                feeName: "F",
                percentage: 0.1m,
                appliedAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var risk = new PolicyAppliedRiskFactor(
                policy.Id, 
                Guid.NewGuid(), 
                RiskFactorLevel.City, 
                Guid.NewGuid(), 
                null, 
                0.1m,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.Throws<ArgumentOutOfRangeException>(() => policy.SetPricing(0m, new[] { fee }, new[] { risk }));
        }

        [Fact]
        public void SetPricing_ShouldSetFinalPremium_AndAppliedFees_WhenValid()
        {
            var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var policy = Policy.CreateDraft(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start, end, 100m);

            var fee1 = new PolicyAppliedFee(
                policyId: policy.Id,
                feeConfigurationId: Guid.NewGuid(),
                feeName: "Broker fee",
                percentage: 0.1m,
                appliedAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var fee2 = new PolicyAppliedFee(
                policyId: policy.Id,
                feeConfigurationId: Guid.NewGuid(),
                feeName: "Risk fee",
                percentage: 0.05m,
                appliedAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var risk1 = new PolicyAppliedRiskFactor(
                policy.Id, 
                Guid.NewGuid(), 
                RiskFactorLevel.City, 
                Guid.NewGuid(), 
                null, 
                0.1m,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var risk2 = new PolicyAppliedRiskFactor(
                policy.Id, 
                Guid.NewGuid(), 
                RiskFactorLevel.BuildingType, 
                null, 
                BuildingType.Industrial, 
                0.05m,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            policy.SetPricing(115m, new[] { fee1, fee2 }, new[] { risk1, risk2 });

            Assert.Equal(115m, policy.FinalPremium);
            Assert.Equal(2, policy.AppliedFees.Count);
            Assert.Equal(2, policy.AppliedRiskFactors.Count);
            Assert.Contains(policy.AppliedRiskFactors, r => r.Level == RiskFactorLevel.City);
            Assert.Contains(policy.AppliedRiskFactors, r => r.Level == RiskFactorLevel.BuildingType);
        }
    }
}