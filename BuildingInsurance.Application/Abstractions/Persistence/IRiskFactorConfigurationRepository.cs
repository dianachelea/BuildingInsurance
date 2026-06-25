using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IRiskFactorConfigurationRepository : IRepository<RiskFactorConfiguration>
    {
        Task<(IReadOnlyList<RiskFactorConfiguration> Items, int TotalCount)> SearchPagedAsync(
            RiskFactorLevel? level,
            Guid? referenceId,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken ct = default);
        Task<IReadOnlyList<RiskFactorConfiguration>> GetActiveForAsync(Guid? geographicId, BuildingType? buildingType, CancellationToken ct = default);
        Task<RiskFactorConfiguration?> GetByTargetAsync(RiskFactorLevel level, Guid? referenceId, BuildingType? buildingType, CancellationToken ct = default);
    }
}