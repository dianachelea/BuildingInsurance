using BuildingInsurance.Domain.Entities.Geography;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface ICountyRepository : IRepository<County>
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task<(IReadOnlyList<County> Items, int TotalCount)> GetByCountryIdPagedAsync(Guid countryId, int page, int pageSize, CancellationToken ct = default);
    }
}