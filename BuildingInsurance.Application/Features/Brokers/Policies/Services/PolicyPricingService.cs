using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Selection;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Policies;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Services
{
    public sealed class PolicyPricingService : IPolicyPricingService
    {
        private readonly IPolicyPricingStrategySelector _selector;

        public PolicyPricingService(IPolicyPricingStrategySelector selector)
        {
            _selector = selector;
        }

        public Task<PolicyPricingResult> CalculateAsync(Policy policy, Building building, CancellationToken ct)
        {
            var strategy = _selector.Select(policy.PolicyStatus);
            return strategy.CalculateAsync(policy, building, ct);
        }
    }
}