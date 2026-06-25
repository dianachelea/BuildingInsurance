namespace BuildingInsurance.Application.Features.Common.Models
{
    public sealed record AppliedFeeSnapshot(Guid FeeConfigurationId, string FeeName, decimal Percentage);
}