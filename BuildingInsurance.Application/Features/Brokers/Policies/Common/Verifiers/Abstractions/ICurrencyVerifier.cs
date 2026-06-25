using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Metadata;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions
{
    public interface ICurrencyVerifier
    {
        Task<Result<Currency>> GetActiveAsync(Guid currencyId, CancellationToken cancellationToken);
    }
}