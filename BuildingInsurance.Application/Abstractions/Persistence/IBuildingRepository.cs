using BuildingInsurance.Domain.Entities.Buildings;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IBuildingRepository : IRepository<Building>
    {
        Task<(IReadOnlyList<Building> Items, int TotalCount)> GetByClientIdPagedAsync(Guid clientId, int page, int pageSize, CancellationToken ct = default); 
        Task<bool> ExistsForClientAtAddressAsync(Guid clientId, Guid cityId, string street, string number, CancellationToken ct = default);
    }
}