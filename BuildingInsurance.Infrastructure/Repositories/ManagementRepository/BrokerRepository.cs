using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.Entities.Management;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.ManagementRepository
{
    public class BrokerRepository : GenericRepository<Broker>, IBrokerRepository
    {
        public BrokerRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public async Task<bool> BrokerCodeExistsAsync(string brokerCode, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(brokerCode))
                return false;

            var normalized = brokerCode.Trim().ToUpperInvariant();

            return await _dbSet
                .AsNoTracking()
                .AnyAsync(b => b.BrokerCode == normalized, ct);
        }

        public async Task<bool> BrokerEmailExistsAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalized = email.Trim().ToLowerInvariant();

            return await _dbSet
                .AsNoTracking()
                .AnyAsync(b => b.ContactInfo.Email == normalized, ct);
        }

        public async Task<(IReadOnlyList<Broker> Items, int TotalCount)> SearchPagedAsync(string? name, bool? isActive, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var normalized = name.Trim();
                query = query.Where(b => b.FullName.Contains(normalized));
            }

            if (isActive is not null)
            {
                var status = isActive.Value ? BrokerStatus.Active : BrokerStatus.Inactive;

                query = query.Where(b => b.BrokerStatus == status);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(b => b.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}