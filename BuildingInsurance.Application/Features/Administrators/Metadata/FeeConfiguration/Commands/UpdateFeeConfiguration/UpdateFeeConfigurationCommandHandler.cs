using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Extensions;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.UpdateFeeConfiguration
{
    public sealed class UpdateFeeConfigurationCommandHandler : IRequestHandler<UpdateFeeConfigurationCommand, Result<FeeConfigurationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateFeeConfigurationCommandHandler> _logger;

        public UpdateFeeConfigurationCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateFeeConfigurationCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<FeeConfigurationDto>> Handle(UpdateFeeConfigurationCommand request, CancellationToken cancellationToken)
        {
            var fee = await _unitOfWork.FeeConfigurations.GetByIdAsync(request.Id, cancellationToken);
            if (fee is null)
            {
                _logger.LogWarning("FeeConfiguration with ID {FeeId} not found.", request.Id);
                return Result<FeeConfigurationDto>.Failure($"Fee configuration with ID {request.Id} not found.", ErrorType.NotFound);
            }

            var fromUtc = request.EffectiveFrom.ToUtc();
            var toUtc = request.EffectiveTo.ToUtc();

            var isUsedInActivePolicies = await _unitOfWork.Policies.IsFeeUsedInActivePoliciesAsync(fee.Id, cancellationToken);

            return isUsedInActivePolicies
                ? await HandleUsedInActivePoliciesAsync(fee, request, fromUtc, toUtc, cancellationToken)
                : await HandleNotUsedInActivePoliciesAsync(fee, request, fromUtc, toUtc, cancellationToken);
        }

        private async Task<Result<FeeConfigurationDto>> HandleNotUsedInActivePoliciesAsync(Domain.Entities.Metadata.FeeConfiguration fee, UpdateFeeConfigurationCommand request, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
        {
            var overlaps = await _unitOfWork.FeeConfigurations.ExistsOverlappingAsync(request.FeeType.MapToDomainFeeType(), request.RiskIndicators.MapToDomainRiskIndicators(), fromUtc, toUtc, cancellationToken, excludeId: fee.Id);

            if (overlaps)
            {
                _logger.LogWarning("Overlapping fee configuration for {FeeType} and {RiskIndicators} between {From} and {To}.", request.FeeType, request.RiskIndicators, fromUtc, toUtc);

                return Result<FeeConfigurationDto>.Conflict("Another fee configuration exists with overlapping period for the same type and risk indicators.");
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                fee.UpdateName(request.Name);
                fee.UpdateTypeAndRisk(request.FeeType.MapToDomainFeeType(), request.RiskIndicators.MapToDomainRiskIndicators());
                fee.UpdatePercentage(request.FeePercentage);
                fee.UpdateValidity(fromUtc, toUtc);

                if (request.IsActive)
                    fee.Activate();
                else
                    fee.Deactivate();

                _unitOfWork.FeeConfigurations.Update(fee);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new FeeConfigurationDto
                {
                    Id = fee.Id,
                    Name = fee.Name,
                    FeeType = fee.FeeType,
                    FeePercentage = fee.FeePercentage,
                    EffectiveFrom = fee.EffectiveFrom,
                    EffectiveTo = fee.EffectiveTo,
                    IsActive = fee.IsActive,
                    RiskIndicators = fee.RiskIndicators
                };

                return Result<FeeConfigurationDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating fee configuration {FeeId}", fee.Id);
                return Result<FeeConfigurationDto>.Failure("Unexpected error while updating fee configuration.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Rollback failed while updating fee configuration {FeeId}", fee.Id); 
                    }
                }
            }
        }

        private async Task<Result<FeeConfigurationDto>> HandleUsedInActivePoliciesAsync(Domain.Entities.Metadata.FeeConfiguration fee, UpdateFeeConfigurationCommand request, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
        {
            if (fee.IsActive && !request.IsActive)
            {
                _logger.LogWarning("Attempt to deactivate fee {FeeId} used by active policies.", fee.Id);
                return Result<FeeConfigurationDto>.Conflict("Cannot deactivate fee configuration because it is referenced by active policies.");
            }

            var changedAffectsPricing =
                fee.FeeType != request.FeeType.MapToDomainFeeType() ||
                fee.FeePercentage != request.FeePercentage ||
                fee.RiskIndicators != request.RiskIndicators.MapToDomainRiskIndicators() ||
                fee.IsActive != request.IsActive ||
                fee.EffectiveFrom != fromUtc ||
                fee.EffectiveTo != toUtc;

            if (changedAffectsPricing)
            {
                _logger.LogWarning("Attempt to change pricing fields of fee {FeeId} used by active policies.", fee.Id);
                return Result<FeeConfigurationDto>.Conflict("Cannot change percentage/type/effective period/risk indicators or deactivate because the fee is used by active policies. " +
                    "You may change the name only, or create a new fee configuration for future periods.");
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                fee.UpdateName(request.Name);
                _unitOfWork.FeeConfigurations.Update(fee);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new FeeConfigurationDto
                {
                    Id = fee.Id,
                    Name = fee.Name,
                    FeeType = fee.FeeType,
                    FeePercentage = fee.FeePercentage,
                    EffectiveFrom = fee.EffectiveFrom,
                    EffectiveTo = fee.EffectiveTo,
                    IsActive = fee.IsActive,
                    RiskIndicators = fee.RiskIndicators
                };

                return Result<FeeConfigurationDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating fee configuration name {FeeId}", fee.Id);
                return Result<FeeConfigurationDto>.Failure("Unexpected error while updating fee configuration name.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Rollback failed while updating fee configuration name {FeeId}", fee.Id); 
                    }
                }
            }
        }
    }
}