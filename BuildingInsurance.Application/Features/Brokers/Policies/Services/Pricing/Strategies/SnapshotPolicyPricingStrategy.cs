using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies
{
    public sealed class SnapshotPolicyPricingStrategy : IPolicyPricingStrategy
    {
        public bool CanHandle(PolicyStatus status)
            => status == PolicyStatus.Active
            || status == PolicyStatus.Cancelled
            || status == PolicyStatus.Expired;

        public Task<PolicyPricingResult> CalculateAsync(Policy policy, Building building, CancellationToken ct)
        {
            var fees = policy.AppliedFees
                .Select(x => new AppliedFeeSnapshot(x.FeeConfigurationId, x.FeeName, x.Percentage))
                .ToList();

            var risks = policy.AppliedRiskFactors
                .Select(x => new AppliedRiskSnapshot(x.RiskFactorConfigurationId, x.Level, x.ReferenceId, x.BuildingType, x.AdjustmentPercentage))
                .ToList();

            var result = new PolicyPricingResult(policy.FinalPremium, fees, risks);
            return Task.FromResult(result);
        }
    }
}