using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class RiskFactorConfigurationDto
    {
        public Guid Id { get; set; }
        public RiskFactorLevel Level { get; set; }
        public Guid? ReferenceId { get; set; }
        public BuildingType? BuildingType { get; set; }
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}