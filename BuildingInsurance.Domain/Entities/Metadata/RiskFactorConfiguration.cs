using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Metadata
{
    public class RiskFactorConfiguration : AggregateRoot, IHasId
    {
        public Guid Id { get; private set; }
        public RiskFactorLevel Level { get; private set; }
        public Guid? ReferenceId { get; private set; }
        public BuildingType? BuildingType { get; private set; }
        public decimal AdjustmentPercentage { get; private set; }
        public bool IsActive { get; private set; }

        private RiskFactorConfiguration() { }

        public RiskFactorConfiguration(RiskFactorLevel level, Guid? referenceId, BuildingType? buildingType, decimal adjustmentPercentage, bool isActive)
        {
            Id = Guid.NewGuid();
            SetLevelAndTarget(level, referenceId, buildingType);
            SetAdjustmentPercentage(adjustmentPercentage);
            IsActive = isActive;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void UpdateAdjustmentPercentage(decimal adjustmentPercentage)
        {
            SetAdjustmentPercentage(adjustmentPercentage);
        }

        public void UpdateTarget(RiskFactorLevel level, Guid? referenceId, BuildingType? buildingType)
        {
            SetLevelAndTarget(level, referenceId, buildingType);
        }

        private void SetAdjustmentPercentage(decimal adjustmentPercentage)
        {
            if (adjustmentPercentage <= -1m || adjustmentPercentage >= 1m)
                throw new ArgumentOutOfRangeException(nameof(adjustmentPercentage), "AdjustmentPercentage must be between -1 and 1 (exclusive).");
            
            if (adjustmentPercentage == 0m)
                throw new ArgumentOutOfRangeException(nameof(adjustmentPercentage), "AdjustmentPercentage cannot be 0.");

            AdjustmentPercentage = adjustmentPercentage;
        }

        private void SetLevelAndTarget(RiskFactorLevel level, Guid? referenceId, BuildingType? buildingType)
        {
            if (!Enum.IsDefined(typeof(RiskFactorLevel), level))
                throw new ArgumentException("Invalid risk factor level.", nameof(level));

            if (level == RiskFactorLevel.BuildingType)
            {
                if (buildingType is null)
                    throw new ArgumentException("BuildingType is required when Level is BuildingType.", nameof(buildingType));

                Level = level;
                BuildingType = buildingType;
                ReferenceId = null;
                return;
            }

            if (referenceId is null || referenceId.Value == Guid.Empty)
                throw new ArgumentException("ReferenceId is required for geographic levels.", nameof(referenceId));

            Level = level;
            ReferenceId = referenceId.Value;
            BuildingType = null;
        }
    }
}