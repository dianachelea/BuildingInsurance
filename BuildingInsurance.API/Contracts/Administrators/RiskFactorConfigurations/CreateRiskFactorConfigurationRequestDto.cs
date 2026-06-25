using BuildingInsurance.API.Contracts.Brokers.Buildings;

namespace BuildingInsurance.API.Contracts.Administrators.RiskFactorConfigurations
{
    public sealed class CreateRiskFactorConfigurationRequestDto
    {
        public RiskFactorLevelRequestDto Level { get; set; }
        public Guid? ReferenceId { get; set; }
        public BuildingTypeRequestDto? BuildingType { get; set; }
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}