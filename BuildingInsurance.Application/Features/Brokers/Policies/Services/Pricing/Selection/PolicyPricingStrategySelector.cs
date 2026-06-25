using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Selection
{
    public sealed class PolicyPricingStrategySelector : IPolicyPricingStrategySelector
    {
        private readonly IEnumerable<IPolicyPricingStrategy> _strategies;

        public PolicyPricingStrategySelector(IEnumerable<IPolicyPricingStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IPolicyPricingStrategy Select(PolicyStatus status)
        {
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(status));
            if (strategy is null)
                throw new InvalidOperationException($"No pricing strategy registered for policy status {status}.");

            return strategy;
        }
    }
}