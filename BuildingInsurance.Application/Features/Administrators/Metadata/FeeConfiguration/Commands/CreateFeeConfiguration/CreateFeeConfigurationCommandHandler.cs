using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Extensions;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.CreateFeeConfiguration
{
    public sealed class CreateFeeConfigurationCommandHandler : IRequestHandler<CreateFeeConfigurationCommand, Result<FeeConfigurationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateFeeConfigurationCommandHandler> _logger;

        public CreateFeeConfigurationCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateFeeConfigurationCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<FeeConfigurationDto>> Handle(CreateFeeConfigurationCommand request, CancellationToken cancellationToken)
        {
            var fromUtc = request.EffectiveFrom.ToUtc();
            var toUtc = request.EffectiveTo.ToUtc();

            var overlaps = await _unitOfWork.FeeConfigurations.ExistsOverlappingAsync(request.FeeType.MapToDomainFeeType(), request.RiskIndicators.MapToDomainRiskIndicators(), fromUtc, toUtc, cancellationToken);

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

                var feeConfig = new Domain.Entities.Metadata.FeeConfiguration(
                    feeName: request.Name,
                    feeType: request.FeeType.MapToDomainFeeType(),
                    feePercentage: request.FeePercentage,
                    effectiveFrom: fromUtc,
                    effectiveTo: toUtc,
                    isActive: request.IsActive,
                    riskIndicators: request.RiskIndicators.MapToDomainRiskIndicators()
                );

                await _unitOfWork.FeeConfigurations.AddAsync(feeConfig, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new FeeConfigurationDto
                {
                    Id = feeConfig.Id,
                    Name = feeConfig.Name,
                    FeeType = feeConfig.FeeType,
                    FeePercentage = feeConfig.FeePercentage,
                    EffectiveFrom = feeConfig.EffectiveFrom,
                    EffectiveTo = feeConfig.EffectiveTo,
                    IsActive = feeConfig.IsActive,
                    RiskIndicators = feeConfig.RiskIndicators
                };

                return Result<FeeConfigurationDto>.Success(dto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during fee configuration creation for {Name}.", request.Name);

                return Result<FeeConfigurationDto>.Failure(error: "Unexpected error during fee configuration creation.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Final rollback attempt failed during fee conguration creation for name = {Name}", request.Name);
                    }
                }
            }
        }
    }
}