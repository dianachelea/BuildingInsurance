using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class FeeConfigurationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FeeType FeeType { get; set; }
        public decimal FeePercentage { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public RiskIndicators RiskIndicators { get; set; }
    }
}