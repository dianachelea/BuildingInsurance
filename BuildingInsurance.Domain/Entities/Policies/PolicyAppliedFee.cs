using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Policies
{
    public class PolicyAppliedFee : IHasId
    {
        public Guid Id { get; private set; }
        public Guid PolicyId { get; private set; }
        public Guid FeeConfigurationId { get; private set; }
        public string FeeName { get; private set; } = null!;
        public decimal Percentage { get; private set; }
        public DateTime AppliedAtUtc { get; private set; }

        private PolicyAppliedFee() { }

        public PolicyAppliedFee(Guid policyId, Guid feeConfigurationId, string feeName, decimal percentage, DateTime appliedAtUtc)
        {
            if (policyId == Guid.Empty) 
                throw new ArgumentException("PolicyId is required.", nameof(policyId));

            if (feeConfigurationId == Guid.Empty) 
                throw new ArgumentException("FeeConfigurationId is required.", nameof(feeConfigurationId));

            if (string.IsNullOrWhiteSpace(feeName)) 
                throw new ArgumentException("FeeName is required.", nameof(feeName));

            if (percentage <= 0m || percentage >= 1m)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 1.");

            if (appliedAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("AppliedAtUtc must be UTC.", nameof(appliedAtUtc));

            Id = Guid.NewGuid();
            PolicyId = policyId;
            FeeConfigurationId = feeConfigurationId;
            FeeName = feeName.Trim();
            Percentage = percentage;
            AppliedAtUtc = appliedAtUtc;
        }
    }
}