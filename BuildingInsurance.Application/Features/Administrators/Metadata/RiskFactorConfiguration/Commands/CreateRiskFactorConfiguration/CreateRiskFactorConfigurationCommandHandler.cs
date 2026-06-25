using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.CreateRiskFactorConfiguration
{
    public sealed class CreateRiskFactorConfigurationCommandHandler : IRequestHandler<CreateRiskFactorConfigurationCommand, Result<RiskFactorConfigurationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRiskFactorTargetVerifier _riskFactorTargetVerifier;
        private readonly ILogger<CreateRiskFactorConfigurationCommandHandler> _logger;

        public CreateRiskFactorConfigurationCommandHandler(IUnitOfWork unitOfWork, IRiskFactorTargetVerifier riskFactorTargetVerifier,ILogger<CreateRiskFactorConfigurationCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _riskFactorTargetVerifier = riskFactorTargetVerifier;
            _logger = logger;
        }
        public async Task<Result<RiskFactorConfigurationDto>> Handle(CreateRiskFactorConfigurationCommand request, CancellationToken cancellationToken)
        {
            var validation = await _riskFactorTargetVerifier.VerifyExistsAsync<RiskFactorConfigurationDto>(request, cancellationToken);
            
            if (validation is not null)
                return validation;

            var existing = await _unitOfWork.RiskFactorConfigurations.GetByTargetAsync(request.Level.MapToDomainRiskFactorLevel(), request.ReferenceId, request.BuildingType.MapToDomainBuildingTypeOptional(), cancellationToken);

            if (existing is not null)
            {
                _logger.LogWarning("RiskFactorConfiguration already exists for Level={Level}, ReferenceId={ReferenceId}, BuildingType={BuildingType}", request.Level, request.ReferenceId, request.BuildingType);

                return Result<RiskFactorConfigurationDto>.Conflict("Risk factor configuration already exists for the provided target.");
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                var entity = new Domain.Entities.Metadata.RiskFactorConfiguration(
                    level: request.Level.MapToDomainRiskFactorLevel(),
                    referenceId: request.ReferenceId,
                    buildingType: request.BuildingType.MapToDomainBuildingTypeOptional(),
                    adjustmentPercentage: request.AdjustmentPercentage,
                    isActive: request.IsActive);

                await _unitOfWork.RiskFactorConfigurations.AddAsync(entity, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new RiskFactorConfigurationDto
                {
                    Id = entity.Id,
                    Level = entity.Level,
                    ReferenceId = entity.ReferenceId,
                    BuildingType = entity.BuildingType,
                    AdjustmentPercentage = entity.AdjustmentPercentage,
                    IsActive = entity.IsActive
                };

                return Result<RiskFactorConfigurationDto>.Success(dto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during RiskFactorConfiguration creation for Level={Level}, ReferenceId={ReferenceId}, BuildingType={BuildingType}", request.Level, request.ReferenceId, request.BuildingType);

                return Result<RiskFactorConfigurationDto>.Failure("Unexpected error during RiskFactorConfiguration creation.", ErrorType.Generic);
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try 
                    { 
                        await _unitOfWork.RollbackAsync(cancellationToken); 
                    }
                    catch (Exception rbEx)
                    {
                        _logger.LogWarning(rbEx, "Final rollback attempt failed during RiskFactorConfiguration creation for Level={Level}", request.Level);
                    }
                }
            }
        }
    }
}