using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Models
{
    public sealed record AppliedRiskSnapshot(Guid RiskFactorConfigurationId, RiskFactorLevel Level, Guid? ReferenceId, BuildingType? BuildingType, decimal AdjustmentPercentage);
}