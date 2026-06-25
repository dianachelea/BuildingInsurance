using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Metadata
{
    public class FeeConfiguration : AggregateRoot, IHasId
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public FeeType FeeType { get; private set; }
        public decimal FeePercentage { get; private set; }
        public DateTime EffectiveFrom { get; private set; }
        public DateTime EffectiveTo { get; private set; }
        public bool IsActive { get; private set; }
        public RiskIndicators RiskIndicators { get; private set; }

        private FeeConfiguration() { }
        public FeeConfiguration(Guid id, string feeName, FeeType feeType, decimal feePercentage, DateTime effectiveFrom, DateTime effectiveTo, bool isActive, RiskIndicators riskIndicators)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetFeeName(feeName);
            SetTypeAndRisk(feeType, riskIndicators);
            SetFeePercentage(feePercentage);
            SetValidity(effectiveFrom, effectiveTo);
            IsActive = isActive;
        }

        public FeeConfiguration(string feeName, FeeType feeType, decimal feePercentage, DateTime effectiveFrom, DateTime effectiveTo, bool isActive, RiskIndicators riskIndicators)
        {
            Id = Guid.NewGuid();
            SetFeeName(feeName);
            SetTypeAndRisk(feeType, riskIndicators);
            SetFeePercentage(feePercentage);
            SetValidity(effectiveFrom, effectiveTo);
            IsActive = isActive;
        }

        public bool IsEffectiveAt(DateTime dateUtc)
        {
            if (!IsActive) 
                return false;

            if (dateUtc < EffectiveFrom) 
                return false;

            if (dateUtc > EffectiveTo)
                return false;

            return true;
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;

        public void UpdatePercentage(decimal feePercentage)
        {
            SetFeePercentage(feePercentage);
        }

        public void UpdateName(string feeName)
        {
            SetFeeName(feeName);
        }

        public void UpdateTypeAndRisk(FeeType feeType, RiskIndicators riskIndicators)
        {
            SetTypeAndRisk(feeType, riskIndicators);
        }

        public void UpdateValidity(DateTime effectiveFrom, DateTime effectiveTo)
        {
            SetValidity(effectiveFrom, effectiveTo);
        }

        public void UpdateRisk(RiskIndicators riskIndicators)
        {
            if (FeeType != FeeType.RiskAdjustment)
                throw new InvalidOperationException("Only RiskAdjustment fees can have risk indicators.");

            if (riskIndicators == RiskIndicators.None)
                throw new ArgumentException("Risk fee must have at least one risk indicator.");

            RiskIndicators = riskIndicators;
        }

        private void SetTypeAndRisk(FeeType feeType, RiskIndicators riskIndicators)
        {
            if (!Enum.IsDefined(typeof(FeeType), feeType))
                throw new ArgumentException("Invalid fee type.", nameof(feeType));

            if (feeType == FeeType.RiskAdjustment)
            {
                if (riskIndicators == RiskIndicators.None)
                    throw new ArgumentException("Risk fee must specify risk indicators.");

                RiskIndicators = riskIndicators;
            }
            else
            {
                if (riskIndicators != RiskIndicators.None)
                    throw new ArgumentException("Only RiskAdjustment fees can have risk indicators.");

                RiskIndicators = RiskIndicators.None;
            }

            FeeType = feeType;
        }

        private void SetValidity(DateTime effectiveFrom, DateTime effectiveTo)
        {
            if (effectiveFrom.Kind != DateTimeKind.Utc)
                throw new ArgumentException("EffectiveFrom must be UTC.");

            if (effectiveTo.Kind != DateTimeKind.Utc)
                throw new ArgumentException("EffectiveTo must be UTC.", nameof(effectiveTo));

            if (effectiveTo <= effectiveFrom)
                throw new ArgumentException("EffectiveTo must be after EffectiveFrom.");

            EffectiveFrom = effectiveFrom;
            EffectiveTo = effectiveTo;
        }

        private void SetFeePercentage(decimal feePercentage)
        {
            if (feePercentage <= 0m || feePercentage >= 1m)
                throw new ArgumentException("Fee percentage must be between 0 and 1.", nameof(feePercentage));

            FeePercentage = feePercentage;
        }

        private void SetFeeName(string feeName)
        {
            if (string.IsNullOrWhiteSpace(feeName))
                throw new ArgumentException("Fee name cannot be null or empty.", nameof(feeName));

            Name = feeName.Trim();
        }
    }
}