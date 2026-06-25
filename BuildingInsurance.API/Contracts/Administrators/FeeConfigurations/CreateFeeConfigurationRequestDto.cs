using BuildingInsurance.API.Contracts.Brokers.Buildings;

namespace BuildingInsurance.API.Contracts.Administrators.FeeConfigurations
{
    public sealed class CreateFeeConfigurationRequestDto
    {
        public string Name { get; set; } = null!;
        public FeeTypeRequestDto FeeType { get; set; }
        public decimal FeePercentage { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public RiskIndicatorsRequestDto RiskIndicators { get; set; }
    }
}