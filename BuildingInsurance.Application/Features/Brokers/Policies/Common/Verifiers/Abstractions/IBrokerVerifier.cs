using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Management;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions
{
    public interface IBrokerVerifier
    {
        Task<Result<Broker>> GetActiveAsync(Guid brokerId, CancellationToken cancellationToken);
    }
}