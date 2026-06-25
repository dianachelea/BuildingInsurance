using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IRepository<T> where T : IHasId
    {
        Task AddAsync(T entry, CancellationToken ct = default);
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
        void Update(T entry);
        void Delete(T entry);
    }
}