using BuildingInsurance.Domain.Entities.Clients;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByIdentificationNumber(string identifier, CancellationToken ct = default);
        Task<bool> IdentificationNumberExistsAsync(string identifier, CancellationToken ct = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
        Task<(IReadOnlyList<Client> Items, int TotalCount)> SearchPagedAsync(string? name, string? identifier, int page, int pageSize, CancellationToken ct = default); 
        Task<Client?> GetByIdWithBuildingsAsync(Guid clientId, CancellationToken ct = default);
        Task<bool> EmailExistsForOtherClientAsync(Guid clientId, string email, CancellationToken ct = default);
        Task<bool> IdentificationNumberExistsForOtherClientAsync(Guid clientId, string identifier, CancellationToken ct = default);
    }
}