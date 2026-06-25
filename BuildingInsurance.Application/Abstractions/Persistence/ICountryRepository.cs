using BuildingInsurance.Domain.Entities.Geography;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface ICountryRepository : IRepository<Country>
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task<(IReadOnlyList<Country> Items, int TotalCount)> GetAllCountriesPagedAsync(int page, int pageSize, CancellationToken ct = default);
    }
}