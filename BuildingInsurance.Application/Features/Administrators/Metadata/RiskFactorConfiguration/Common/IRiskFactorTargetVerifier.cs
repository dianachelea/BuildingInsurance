using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common
{
    public interface IRiskFactorTargetVerifier
    {
        Task<Result<TDto>?> VerifyExistsAsync<TDto>(IRiskFactorTargetRequest request, CancellationToken ct);
    }
}