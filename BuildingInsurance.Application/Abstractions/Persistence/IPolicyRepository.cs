using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<Policy?> GetDetailsAsync(Guid policyId, CancellationToken ct = default);
        Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchPagedAsync(
            Guid? clientId, 
            Guid? brokerId, 
            PolicyStatus? status, 
            DateTime? startDate, 
            DateTime? endDate, 
            int page, 
            int pageSize, 
            CancellationToken ct = default);
        Task<bool> HasOverlappingActivePolicyAsync(Guid buildingId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
        Task<bool> IsFeeUsedInActivePoliciesAsync(Guid feeConfigurationId, CancellationToken ct = default);
        Task<bool> IsRiskFactorUsedInActivePoliciesAsync(Guid riskFactorId, CancellationToken ct = default);
        Task<int> MarkExpiredAsync(DateTime nowUtc, CancellationToken ct = default);
        Task ReplaceAppliedPricingAsync(Policy policy, IReadOnlyList<PolicyAppliedFee> newFees, IReadOnlyList<PolicyAppliedRiskFactor> newRisks, CancellationToken ct = default);
        Task<Policy?> GetForActivationAsync(Guid policyId, CancellationToken ct = default);
    }
}