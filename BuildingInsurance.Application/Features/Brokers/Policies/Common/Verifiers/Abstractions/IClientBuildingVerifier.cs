using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Clients;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions
{
    public interface IClientBuildingVerifier
    {
        Task<Result<(Client Client, Building Building)>> GetAndVerifyAsync(Guid clientId, Guid buildingId, CancellationToken cancellationToken);
    }
}