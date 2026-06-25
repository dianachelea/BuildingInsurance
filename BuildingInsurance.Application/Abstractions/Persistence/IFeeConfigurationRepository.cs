using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IFeeConfigurationRepository : IRepository<FeeConfiguration>
    {
        Task<(IReadOnlyList<FeeConfiguration> Items, int TotalCount)> SearchPagedAsync(string? name, FeeType? type, bool? isActive, int page, int pageSize, CancellationToken ct = default);
        Task<bool> ExistsOverlappingAsync(FeeType feeType, RiskIndicators riskIndicators, DateTime effectiveFrom, DateTime effectiveTo, CancellationToken ct = default, Guid? excludeId = null);
    }
}