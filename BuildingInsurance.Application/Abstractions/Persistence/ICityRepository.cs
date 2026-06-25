using BuildingInsurance.Domain.Entities.Geography;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface ICityRepository : IRepository<City>
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task<(IReadOnlyList<City> Items, int TotalCount)> GetByCountyIdPagedAsync(Guid countyId, int page, int pageSize, CancellationToken ct = default);
    }
}