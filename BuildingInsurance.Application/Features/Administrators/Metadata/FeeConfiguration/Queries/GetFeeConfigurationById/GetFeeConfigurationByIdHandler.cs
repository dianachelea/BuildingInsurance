using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.GetFeeConfigurationById
{
    public sealed class GetFeeConfigurationByIdHandler : IRequestHandler<GetFeeConfigurationByIdQuery, Result<FeeConfigurationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetFeeConfigurationByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<FeeConfigurationDto>> Handle(GetFeeConfigurationByIdQuery request, CancellationToken cancellationToken)
        {
            var feeConfiguration = await _unitOfWork.FeeConfigurations.GetByIdAsync(request.FeeConfigurationId, cancellationToken);
            if (feeConfiguration is null)
            {
                return Result<FeeConfigurationDto>.Failure("Fee configuration not found.", ErrorType.NotFound);
            }

            var feeConfigurationDto = new FeeConfigurationDto
            {
                Id = feeConfiguration.Id,
                Name = feeConfiguration.Name,
                FeeType = feeConfiguration.FeeType,
                FeePercentage = feeConfiguration.FeePercentage,
                EffectiveFrom = feeConfiguration.EffectiveFrom,
                EffectiveTo = feeConfiguration.EffectiveTo,
                IsActive = feeConfiguration.IsActive,
                RiskIndicators = feeConfiguration.RiskIndicators
            };

            return Result<FeeConfigurationDto>.Success(feeConfigurationDto);
        }
    }
}