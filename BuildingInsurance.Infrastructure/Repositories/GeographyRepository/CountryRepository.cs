using BuildingInsurance.Domain.Entities.Geography;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.GeographyRepository
{
    public class CountryRepository : GenericRepository<Country>, ICountryRepository
    {
        public CountryRepository(BuildingInsuranceDbContext db) : base(db)
        {

        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        {
            return _db.Countries.AsNoTracking().AnyAsync(c => c.Id == id, ct);
        }

        public async Task<(IReadOnlyList<Country> Items, int TotalCount)> GetAllCountriesPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Countries.AsNoTracking();

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