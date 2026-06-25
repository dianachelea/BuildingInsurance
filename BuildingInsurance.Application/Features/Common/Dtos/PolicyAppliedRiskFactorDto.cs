using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class PolicyAppliedRiskFactorDto
    {
        public Guid RiskFactorConfigurationId { get; init; }
        public RiskFactorLevel Level { get; init; }
        public Guid? ReferenceId { get; init; }
        public BuildingType? BuildingType { get; init; }
        public decimal AdjustmentPercentage { get; init; }
        public DateTime AppliedAtUtc { get; init; }
    }
}