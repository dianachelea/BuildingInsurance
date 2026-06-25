using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common
{
    public sealed class RiskFactorTargetVerifier : IRiskFactorTargetVerifier
    {
        private readonly IUnitOfWork _unitOfWork;

        public RiskFactorTargetVerifier(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TDto>?> VerifyExistsAsync<TDto>(IRiskFactorTargetRequest request, CancellationToken ct)
        {
            if(request.Level != RiskFactorLevelContract.BuildingType)
            {
                var id = request.ReferenceId!.Value;

                if (request.Level == RiskFactorLevelContract.Country)
                {
                    if (!await _unitOfWork.Countries.ExistsAsync(id, ct))
                        return Result<TDto>.Failure("Country not found.", ErrorType.NotFound);
                }
                else if (request.Level == RiskFactorLevelContract.County)
                {
                    if (!await _unitOfWork.Counties.ExistsAsync(id, ct))
                        return Result<TDto>.Failure("County not found.", ErrorType.NotFound);
                }
                else if (request.Level == RiskFactorLevelContract.City)
                {
                    if (!await _unitOfWork.Cities.ExistsAsync(id, ct))
                        return Result<TDto>.Failure("City not found.", ErrorType.NotFound);
                }
                else
                {
                    return Result<TDto>.Failure("Invalid risk factor level.", ErrorType.Validation);
                }
            }
            return null;
        }
    }
}