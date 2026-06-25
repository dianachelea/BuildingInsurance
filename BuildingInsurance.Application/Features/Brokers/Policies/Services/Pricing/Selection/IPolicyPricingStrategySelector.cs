using BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Selection
{
    public interface IPolicyPricingStrategySelector
    {
        IPolicyPricingStrategy Select(PolicyStatus status);
    }
}