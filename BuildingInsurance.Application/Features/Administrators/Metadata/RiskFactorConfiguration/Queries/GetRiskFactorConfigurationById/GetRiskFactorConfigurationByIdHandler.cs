using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.GetRiskFactorConfigurationById
{
    public sealed class GetRiskFactorConfigurationByIdHandler : IRequestHandler<GetRiskFactorConfigurationByIdQuery, Result<RiskFactorConfigurationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetRiskFactorConfigurationByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RiskFactorConfigurationDto>> Handle(GetRiskFactorConfigurationByIdQuery request, CancellationToken cancellationToken)
        {
            var config = await _unitOfWork.RiskFactorConfigurations.GetByIdAsync(request.RiskFactorConfigurationId, cancellationToken);
            if (config is null)
                return Result<RiskFactorConfigurationDto>.Failure("Risk factor configuration not found.", ErrorType.NotFound);

            var dto = new RiskFactorConfigurationDto
            {
                Id = config.Id,
                Level = config.Level,
                ReferenceId = config.ReferenceId,
                BuildingType = config.BuildingType,
                AdjustmentPercentage = config.AdjustmentPercentage,
                IsActive = config.IsActive
            };

            return Result<RiskFactorConfigurationDto>.Success(dto);
        }
    }
}