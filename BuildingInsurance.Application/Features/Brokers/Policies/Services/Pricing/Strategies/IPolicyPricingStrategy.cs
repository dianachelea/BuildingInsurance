using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies
{
    public interface IPolicyPricingStrategy
    {
        bool CanHandle(PolicyStatus status);

        Task<PolicyPricingResult> CalculateAsync(Policy policy, Building building, CancellationToken ct);
    }
}