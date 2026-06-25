using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.MetadataRepository
{
    public class RiskFactorConfigurationRepository : GenericRepository<RiskFactorConfiguration>, IRiskFactorConfigurationRepository
    {
        public RiskFactorConfigurationRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public async Task<IReadOnlyList<RiskFactorConfiguration>> GetActiveForAsync(Guid? geographicId, BuildingType? buildingType, CancellationToken ct = default)
        {
            if ((geographicId is null || geographicId.Value == Guid.Empty) && buildingType is null)
                return Array.Empty<RiskFactorConfiguration>();

            var query = _dbSet.AsNoTracking().Where(r => r.IsActive);

            var hasGeo = geographicId is not null && geographicId.Value != Guid.Empty;
            var hasBuildingType = buildingType is not null;

            if (hasGeo)
            {
                query = query.Where(r =>
                r.Level != RiskFactorLevel.BuildingType &&
                r.ReferenceId == geographicId!.Value);
            }

            if (hasBuildingType)
            {
                query = query.Where(r =>
                    r.Level == RiskFactorLevel.BuildingType &&
                    r.BuildingType == buildingType);
            }

            return await query
                .OrderBy(r => r.Level)
                .ThenBy(r => r.ReferenceId)
                .ThenBy(r => r.BuildingType)
                .ToListAsync(ct);
        }

        public Task<RiskFactorConfiguration?> GetByTargetAsync(RiskFactorLevel level, Guid? referenceId, BuildingType? buildingType, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();
            query = query.Where(r => r.Level == level);

            if (level == RiskFactorLevel.BuildingType)
            {
                query = query.Where(r => r.BuildingType == buildingType);
            }
            else
            {
                query = query.Where(r => r.ReferenceId == referenceId);
            }
            return query.FirstOrDefaultAsync(ct);
        }

        public async Task<(IReadOnlyList<RiskFactorConfiguration> Items, int TotalCount)> SearchPagedAsync(RiskFactorLevel? level, Guid? referenceId, bool? isActive, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (level is not null)
                query = query.Where(r => r.Level == level.Value);

            if (referenceId is not null && referenceId.Value != Guid.Empty)
                query = query.Where(r => r.ReferenceId == referenceId.Value);

            if (isActive is not null)
                query = query.Where(r => r.IsActive == isActive.Value);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(r => r.IsActive)
                .ThenBy(r => r.Level)
                .ThenBy(r => r.ReferenceId)
                .ThenBy(r => r.BuildingType)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}