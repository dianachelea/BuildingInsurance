using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.MetadataRepository
{
    public class CurrencyRepository : GenericRepository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public async Task<Currency?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Code == code, ct);
        }

        public async Task<bool> IsUsedInActivePoliciesAsync(Guid currencyId, CancellationToken ct = default)
        {
            if (currencyId == Guid.Empty)
                throw new ArgumentException("currencyId is required.", nameof(currencyId));

            return await _db.Policies
                .AsNoTracking()
                .AnyAsync(p => p.CurrencyId == currencyId && p.PolicyStatus == PolicyStatus.Active, ct);
        }

        public async Task<(IReadOnlyList<Currency> Items, int TotalCount)> SearchPagedAsync(string? name, bool? isActive, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var normalized = name.Trim();
                query = query.Where(c => c.Name.Contains(normalized));
            }

            if (isActive is not null)
                query = query.Where(c => c.IsActive == isActive.Value);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(c => c.Code)
                .ThenBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}