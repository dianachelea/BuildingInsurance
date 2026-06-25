namespace BuildingInsurance.Application.Features.Common.Models
{
    public sealed record PolicyPricingResult(decimal FinalPremium, IReadOnlyList<AppliedFeeSnapshot> Fees, IReadOnlyList<AppliedRiskSnapshot> Risks);
}