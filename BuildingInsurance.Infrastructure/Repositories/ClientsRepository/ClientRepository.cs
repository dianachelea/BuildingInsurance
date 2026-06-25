using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.ClientsRepository
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(BuildingInsuranceDbContext db) : base(db)
        {
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            var normalizezEmail = email.Trim().ToLowerInvariant();
            return await _dbSet.AsNoTracking().AnyAsync(c => c.ContactInfo.Email == normalizezEmail, ct);
        }

        public async Task<Client?> GetByIdentificationNumber(string identifier, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return null;

            var normalizedIdNumber = identifier.Trim();
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.PersonalIdentificationNumber == normalizedIdNumber || c.CompanyRegistrationNumber == normalizedIdNumber, ct);
        }

        public async Task<Client?> GetByIdWithBuildingsAsync(Guid clientId, CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId, ct);
        }

        public async Task<bool> IdentificationNumberExistsAsync(string identifier, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            var normalizedIdNumber = identifier.Trim();

            return await _db.Clients.AsNoTracking().AnyAsync(c => c.PersonalIdentificationNumber == normalizedIdNumber || c.CompanyRegistrationNumber == normalizedIdNumber, ct);
        }

        public async Task<(IReadOnlyList<Client> Items, int TotalCount)> SearchPagedAsync(string? name, string? identifier, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Clients.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                var normalizedName = name.Trim();
                query = query.Where(c=>c.FullName.Contains(normalizedName));
            }
            if(!string.IsNullOrWhiteSpace(identifier))
            {
                var normalizedIdNumber = identifier.Trim();
                query = query.Where(c => c.PersonalIdentificationNumber == normalizedIdNumber || c.CompanyRegistrationNumber == normalizedIdNumber);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(c => c.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<bool> EmailExistsForOtherClientAsync(Guid clientId, string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalized = email.Trim();

            return await _dbSet.AsNoTracking().AnyAsync(c =>
                c.Id != clientId &&
                c.ContactInfo.Email == normalized,
                ct);
        }

        public async Task<bool> IdentificationNumberExistsForOtherClientAsync(Guid clientId, string identifier, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            var normalized = identifier.Trim();

            return await _dbSet.AsNoTracking().AnyAsync(c =>
                c.Id != clientId &&
                (c.PersonalIdentificationNumber == normalized || c.CompanyRegistrationNumber == normalized),
                ct);
        }
    }
}