using BuildingInsurance.Domain.Entities.Management;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IBrokerRepository : IRepository<Broker>
    {
        Task<bool> BrokerCodeExistsAsync(string brokerCode, CancellationToken ct = default);
        Task<bool> BrokerEmailExistsAsync(string email, CancellationToken ct = default);
        Task<(IReadOnlyList<Broker> Items, int TotalCount)> SearchPagedAsync(string? name, bool? isActive, int page, int pageSize, CancellationToken ct = default);
    }
}