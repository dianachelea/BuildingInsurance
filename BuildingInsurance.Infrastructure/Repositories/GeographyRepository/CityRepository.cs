using BuildingInsurance.Domain.Entities.Geography;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.GeographyRepository
{
    public class CityRepository : GenericRepository<City>, ICityRepository
    {
        public CityRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        {
            return _dbSet.AsNoTracking().AnyAsync(c => c.Id == id, ct);
        }

        public async Task<(IReadOnlyList<City> Items, int TotalCount)> GetByCountyIdPagedAsync(Guid countyId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().Where(c => c.CountyId == countyId);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}