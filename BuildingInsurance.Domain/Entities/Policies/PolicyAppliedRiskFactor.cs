using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Policies
{
    public sealed class PolicyAppliedRiskFactor : IHasId
    {
        public Guid Id { get; private set; }
        public Guid PolicyId { get; private set; }
        public Guid RiskFactorConfigurationId { get; private set; }
        public RiskFactorLevel Level { get; private set; }
        public Guid? ReferenceId { get; private set; }
        public BuildingType? BuildingType { get; private set; }
        public decimal AdjustmentPercentage { get; private set; }
        public DateTime AppliedAtUtc { get; private set; }

        private PolicyAppliedRiskFactor() { }

        public PolicyAppliedRiskFactor(Guid policyId, Guid riskFactorConfigurationId, RiskFactorLevel level, Guid? referenceId, BuildingType? buildingType, decimal adjustmentPercentage, DateTime appliedAtUtc)
        {
            if (policyId == Guid.Empty)
                throw new ArgumentException("PolicyId is required.", nameof(policyId));

            if (riskFactorConfigurationId == Guid.Empty)
                throw new ArgumentException("RiskFactorConfigurationId is required.", nameof(riskFactorConfigurationId));

            if (!Enum.IsDefined(typeof(RiskFactorLevel), level))
                throw new ArgumentException("Invalid risk factor level.", nameof(level));

            if (level == RiskFactorLevel.BuildingType)
            {
                if (buildingType is null)
                    throw new ArgumentException("BuildingType is required for BuildingType level.", nameof(buildingType));
                referenceId = null;
            }
            else
            {
                if (referenceId is null || referenceId.Value == Guid.Empty)
                    throw new ArgumentException("ReferenceId is required for geographic levels.", nameof(referenceId));
                buildingType = null;
            }

            if (adjustmentPercentage <= -1m || adjustmentPercentage >= 1m)
                throw new ArgumentOutOfRangeException(nameof(adjustmentPercentage), "AdjustmentPercentage must be greater than -1 and less than 1.");

            if (appliedAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("AppliedAtUtc must be UTC.", nameof(appliedAtUtc));

            Id = Guid.NewGuid();
            PolicyId = policyId;
            RiskFactorConfigurationId = riskFactorConfigurationId;
            Level = level;
            ReferenceId = referenceId;
            BuildingType = buildingType;
            AdjustmentPercentage = adjustmentPercentage;
            AppliedAtUtc = appliedAtUtc;
        }
    }
}