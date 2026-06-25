using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        Task<Currency?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<bool> IsUsedInActivePoliciesAsync(Guid currencyId, CancellationToken ct = default);
        Task<(IReadOnlyList<Currency> Items, int TotalCount)> SearchPagedAsync(
            string? name,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken ct = default);
    }
}