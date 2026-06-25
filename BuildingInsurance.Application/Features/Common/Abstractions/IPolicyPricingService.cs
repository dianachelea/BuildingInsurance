using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Policies;

namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface IPolicyPricingService
    {
        Task<PolicyPricingResult> CalculateAsync(Policy policy, Building building, CancellationToken ct);
    }
}