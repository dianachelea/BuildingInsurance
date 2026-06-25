using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.BuildingsRepository
{
    public class BuildingRepository : GenericRepository<Building>, IBuildingRepository
    {
        public BuildingRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public async Task<bool> ExistsForClientAtAddressAsync(Guid clientId, Guid cityId, string street, string number, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(number))
                return false;

            var normalizedStreet = street.Trim().ToUpperInvariant();
            var normalizedNumber = number.Trim().ToUpperInvariant();

            return await _db.Buildings.AsNoTracking().AnyAsync(b => b.ClientId == clientId && b.CityId == cityId && b.Address.Street == normalizedStreet && b.Address.Number == normalizedNumber, ct);
        }

        public async Task<(IReadOnlyList<Building> Items, int TotalCount)> GetByClientIdPagedAsync(Guid clientId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().Where(b => b.ClientId == clientId);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}