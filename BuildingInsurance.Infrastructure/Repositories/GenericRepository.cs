using BuildingInsurance.Domain.Interfaces;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class, IHasId
    {
        protected readonly BuildingInsuranceDbContext _db;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(BuildingInsuranceDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }

        public virtual async Task AddAsync(T entry, CancellationToken ct = default)
        {
            await _dbSet.AddAsync(entry, ct);
        }

        public void Delete(T entry)
        {
            _dbSet.Remove(entry);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public void Update(T entry)
        {
            _dbSet.Update(entry);
        }
    }
}
