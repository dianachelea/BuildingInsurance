using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.UpdateRiskFactorConfiguration
{
    public sealed class UpdateRiskFactorConfigurationCommandHandler : IRequestHandler<UpdateRiskFactorConfigurationCommand, Result<RiskFactorConfigurationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRiskFactorTargetVerifier _riskFactorTargetValidator;
        private readonly ILogger<UpdateRiskFactorConfigurationCommandHandler> _logger;

        public UpdateRiskFactorConfigurationCommandHandler(IUnitOfWork unitOfWork, IRiskFactorTargetVerifier riskFactorTargetValidator, ILogger<UpdateRiskFactorConfigurationCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _riskFactorTargetValidator = riskFactorTargetValidator;
            _logger = logger;
        }

        public async Task<Result<RiskFactorConfigurationDto>> Handle(UpdateRiskFactorConfigurationCommand request, CancellationToken cancellationToken)
        {
            var riskFactor = await _unitOfWork.RiskFactorConfigurations.GetByIdAsync(request.Id, cancellationToken);
            if (riskFactor is null)
            {
                _logger.LogWarning("RiskFactorConfiguration with ID {RiskFactorId} not found.", request.Id);
                return Result<RiskFactorConfigurationDto>.Failure($"Risk factor configuration with ID {request.Id} not found.", ErrorType.NotFound);
            }

            var validation = await _riskFactorTargetValidator.VerifyExistsAsync<RiskFactorConfigurationDto>(request, cancellationToken);
            if (validation is not null)
                return validation;

            var isUsedInActivePolicies = await _unitOfWork.Policies.IsRiskFactorUsedInActivePoliciesAsync(riskFactor.Id, cancellationToken);

            if (!isUsedInActivePolicies)
                return await HandleNotUsedInActivePoliciesAsync(riskFactor, request, cancellationToken);

            _logger.LogWarning("Update rejected for risk factor {RiskFactorId} because it is used by active policies.", riskFactor.Id);

            return Result<RiskFactorConfigurationDto>.Conflict("Risk factor configuration is used by active policies and cannot be updated. " +
                "No changes were applied. Create a new risk factor configuration for future use."
            );
        }

        private async Task<Result<RiskFactorConfigurationDto>> HandleNotUsedInActivePoliciesAsync(Domain.Entities.Metadata.RiskFactorConfiguration riskFactor, UpdateRiskFactorConfigurationCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.RiskFactorConfigurations.GetByTargetAsync(request.Level.MapToDomainRiskFactorLevel(), request.ReferenceId, request.BuildingType.MapToDomainBuildingTypeOptional(), cancellationToken);

            if (existing is not null && existing.Id != riskFactor.Id)
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

                riskFactor.UpdateTarget(request.Level.MapToDomainRiskFactorLevel(), request.ReferenceId, request.BuildingType.MapToDomainBuildingTypeOptional());
                riskFactor.UpdateAdjustmentPercentage(request.AdjustmentPercentage);

                if (request.IsActive)
                    riskFactor.Activate();
                else
                    riskFactor.Deactivate();

                _unitOfWork.RiskFactorConfigurations.Update(riskFactor);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new RiskFactorConfigurationDto()
                {
                    Id = riskFactor.Id,
                    Level = riskFactor.Level,
                    ReferenceId = riskFactor.ReferenceId,
                    BuildingType = riskFactor.BuildingType,
                    AdjustmentPercentage = riskFactor.AdjustmentPercentage,
                    IsActive = riskFactor.IsActive
                };

                return Result<RiskFactorConfigurationDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating risk factor configuration {RiskFactorId}", riskFactor.Id);
                return Result<RiskFactorConfigurationDto>.Failure("Unexpected error while updating risk factor configuration.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Rollback failed while updating risk factor configuration {RiskFactorId}", riskFactor.Id);
                    }
                }
            }
        }
    }
}