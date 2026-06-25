using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Extensions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.CancelPolicy
{
    public sealed class CancelPolicyCommandHandler : IRequestHandler<CancelPolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelPolicyCommandHandler> _logger;
        private readonly IBrokerVerifier _brokerVerifier;

        public CancelPolicyCommandHandler(IUnitOfWork unitOfWork, ILogger<CancelPolicyCommandHandler> logger, IBrokerVerifier brokerVerifier)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _brokerVerifier = brokerVerifier;
        }

        public async Task<Result<PolicyDto>> Handle(CancelPolicyCommand request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.Policies.GetByIdAsync(request.PolicyId, cancellationToken);
            if (policy is null)
                return Result<PolicyDto>.Failure($"Policy with ID {request.PolicyId} not found.", ErrorType.NotFound);
            
            if (policy.PolicyStatus == PolicyStatus.Cancelled)
            {
                var dtoAlreadyCancelled = new PolicyDto
                {
                    Id = policy.Id,
                    PolicyNumber = policy.PolicyNumber,
                    ClientId = policy.ClientId,
                    BuildingId = policy.BuildingId,
                    BrokerId = policy.BrokerId,
                    CurrencyId = policy.CurrencyId,
                    PolicyStatus = policy.PolicyStatus,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    BasePremium = policy.BasePremium,
                    FinalPremium = policy.FinalPremium,
                    CancellationEffectiveDate = policy.CancellationEffectiveDate
                };
                return Result<PolicyDto>.Success(dtoAlreadyCancelled);
            }

            var brokerResult = await _brokerVerifier.GetActiveAsync(policy.BrokerId, cancellationToken);
            if (!brokerResult.IsSuccess)
                return Result<PolicyDto>.Failure(brokerResult.Error, brokerResult.ErrorType);

            var cancellationUtc = request.CancellationEffectiveDate.ToUtc();

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                policy.Cancel(request.Reason, cancellationUtc);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new PolicyDto
                {
                    Id = policy.Id,
                    PolicyNumber = policy.PolicyNumber,
                    ClientId = policy.ClientId,
                    BuildingId = policy.BuildingId,
                    BrokerId = policy.BrokerId,
                    CurrencyId = policy.CurrencyId,
                    PolicyStatus = policy.PolicyStatus,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    BasePremium = policy.BasePremium,
                    FinalPremium = policy.FinalPremium,
                    CancellationEffectiveDate = policy.CancellationEffectiveDate
                };

                return Result<PolicyDto>.Success(dto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error cancelling policy. PolicyId={PolicyId}", request.PolicyId);

                string publicMessage;

                if (policy.PolicyStatus != PolicyStatus.Active)
                {
                    publicMessage = "Only Active policies can be cancelled.";
                }
                else if (cancellationUtc < policy.StartDate)
                {
                    publicMessage = "Cancellation effective date cannot be before StartDate.";
                }
                else if (cancellationUtc > policy.EndDate)
                {
                    publicMessage = "Cancellation effective date cannot be after EndDate.";
                }
                else
                {
                    publicMessage = "Policy cannot be cancelled due to invalid state or reason.";
                }

                return Result<PolicyDto>.Failure(publicMessage, ErrorType.Validation);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error cancelling policy. PolicyId={PolicyId}", request.PolicyId);
                return Result<PolicyDto>.Failure("Validation error activating policy.", ErrorType.Validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling policy. PolicyId={PolicyId}", request.PolicyId);
                return Result<PolicyDto>.Failure("An error occurred while cancelling the policy.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Rollback failed during policy cancellation. PolicyId={PolicyId}", request.PolicyId);
                    }
                }
            }
        }
    }
}