using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.MetadataRepository
{
    public class FeeConfigurationRepository : GenericRepository<FeeConfiguration>, IFeeConfigurationRepository
    {
        public FeeConfigurationRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public async Task<bool> ExistsOverlappingAsync(FeeType feeType, RiskIndicators riskIndicators, DateTime effectiveFrom, DateTime effectiveTo, CancellationToken ct = default, Guid? excludeId = null)
        {
            if (effectiveFrom.Kind != DateTimeKind.Utc)
                throw new ArgumentException("effectiveFrom must be UTC.", nameof(effectiveFrom));

            if (effectiveTo.Kind != DateTimeKind.Utc)
                throw new ArgumentException("toUtc must be UTC.", nameof(effectiveTo));

            return await _dbSet.AnyAsync(f =>
            f.FeeType == feeType &&
            f.RiskIndicators == riskIndicators &&
            f.EffectiveFrom < effectiveTo &&
            f.EffectiveTo > effectiveFrom &&
            (excludeId == null || f.Id != excludeId.Value),
            ct);
        }

        public async Task<(IReadOnlyList<FeeConfiguration> Items, int TotalCount)> SearchPagedAsync(string? name, FeeType? type, bool? isActive, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var normalized = name.Trim();
                query = query.Where(f => f.Name.Contains(normalized));
            }

            if (type is not null)
                query = query.Where(f => f.FeeType == type.Value);

            if (isActive is not null)
                query = query.Where(f => f.IsActive == isActive.Value);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(f => f.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}