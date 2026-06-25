using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.PolicyRepository
{
    public class PolicyRepository : GenericRepository<Policy>, IPolicyRepository
    {
        public PolicyRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public async Task<Policy?> GetDetailsAsync(Guid policyId, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking()
                .Include(p => p.AppliedFees)
                .Include(p => p.AppliedRiskFactors)
                .FirstOrDefaultAsync(p => p.Id == policyId, ct);
        }

        public async Task<bool> HasOverlappingActivePolicyAsync(Guid buildingId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            if (buildingId == Guid.Empty)
                throw new ArgumentException("buildingId is required.", nameof(buildingId));
            
            return await _dbSet.AnyAsync(p =>
            p.BuildingId == buildingId &&
            p.PolicyStatus == PolicyStatus.Active &&
            p.StartDate < endDate &&
            p.EndDate > startDate,
            ct);
        }

        public async Task<bool> IsFeeUsedInActivePoliciesAsync(Guid feeConfigurationId, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.PolicyStatus == PolicyStatus.Active)
                .AnyAsync(p => p.AppliedFees.Any(af => af.FeeConfigurationId == feeConfigurationId), ct);
        }

        public async Task<bool> IsRiskFactorUsedInActivePoliciesAsync(Guid riskFactorId, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.PolicyStatus == PolicyStatus.Active)
                .AnyAsync(p => p.AppliedRiskFactors.Any(af => af.RiskFactorConfigurationId == riskFactorId), ct);
        }

        public async Task<Policy?> GetForActivationAsync(Guid policyId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(p => p.AppliedFees)
                .Include(p => p.AppliedRiskFactors)
                .FirstOrDefaultAsync(p => p.Id == policyId, ct);
        }

        public async Task ReplaceAppliedPricingAsync(Policy policy, IReadOnlyList<PolicyAppliedFee> newFees, IReadOnlyList<PolicyAppliedRiskFactor> newRisks, CancellationToken ct = default)
        {
            _db.PolicyAppliedFees.RemoveRange(policy.AppliedFees);
            _db.PolicyAppliedRiskFactors.RemoveRange(policy.AppliedRiskFactors);

            await _db.PolicyAppliedFees.AddRangeAsync(newFees, ct);
            await _db.PolicyAppliedRiskFactors.AddRangeAsync(newRisks, ct);
        }

        public async Task<int> MarkExpiredAsync(DateTime nowUtc, CancellationToken ct = default)
        {
            if (nowUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("nowUtc must be UTC.", nameof(nowUtc));

            return await _db.Policies
                .Where(p => p.PolicyStatus == PolicyStatus.Active && p.EndDate < nowUtc)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.PolicyStatus, PolicyStatus.Expired), ct);
        }

        public async Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchPagedAsync(Guid? clientId, Guid? brokerId, PolicyStatus? status, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Policies.AsNoTracking().AsQueryable();
            if(clientId is not null && clientId != Guid.Empty)
            {
                query = query.Where(p => p.ClientId == clientId);
            }

            if(brokerId is not null && brokerId != Guid.Empty)
            {
                query = query.Where(p => p.BrokerId == brokerId);
            }

            if(status is not null)
            {
                query = query.Where(p => p.PolicyStatus == status);
            }

            if(startDate is not null)
            {
                query = query.Where(p => p.EndDate >= startDate);
            }

            if(endDate is not null)
            {
                query = query.Where(p => p.StartDate <= endDate);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(p => p.StartDate)
                .ThenByDescending(p => p.EndDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}