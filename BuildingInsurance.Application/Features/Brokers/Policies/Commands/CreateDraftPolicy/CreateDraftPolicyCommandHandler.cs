using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Extensions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Policies;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy
{
    public sealed class CreateDraftPolicyCommandHandler : IRequestHandler<CreateDraftPolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPolicyPricingService _policyPricingService;
        private readonly ILogger<CreateDraftPolicyCommandHandler> _logger;
        private readonly IClientBuildingVerifier _clientBuildingVerifier;
        private readonly ICurrencyVerifier _currencyVerifier;
        private readonly IBrokerVerifier _brokerVerifier;

        public CreateDraftPolicyCommandHandler(IUnitOfWork unitOfWork, IPolicyPricingService policyPricingService, ILogger<CreateDraftPolicyCommandHandler> logger, IClientBuildingVerifier clientBuildingVerifier, ICurrencyVerifier currencyVerifier, IBrokerVerifier brokerVerifier)
        {
            _unitOfWork = unitOfWork;
            _policyPricingService = policyPricingService;
            _logger = logger;
            _clientBuildingVerifier = clientBuildingVerifier;
            _currencyVerifier = currencyVerifier;
            _brokerVerifier = brokerVerifier;
        }

        public async Task<Result<PolicyDto>> Handle(CreateDraftPolicyCommand request, CancellationToken cancellationToken)
        {
            var startUtc = request.StartDate.ToUtc();
            var endUtc = request.EndDate.ToUtc();

            var clientBuildingResult = await _clientBuildingVerifier.GetAndVerifyAsync(request.ClientId, request.BuildingId, cancellationToken);
            if (!clientBuildingResult.IsSuccess)
            {
                return Result<PolicyDto>.Failure(clientBuildingResult.Error, clientBuildingResult.ErrorType);
            }

            var building = clientBuildingResult.Value.Building;

            var currencyResult = await _currencyVerifier.GetActiveAsync(request.CurrencyId, cancellationToken);
            if (!currencyResult.IsSuccess)
                return Result<PolicyDto>.Failure(currencyResult.Error, currencyResult.ErrorType);

            var brokerResult = await _brokerVerifier.GetActiveAsync(request.BrokerId, cancellationToken);
            if (!brokerResult.IsSuccess)
                return Result<PolicyDto>.Failure(brokerResult.Error, brokerResult.ErrorType);

            if (await _unitOfWork.Policies.HasOverlappingActivePolicyAsync(building.Id, startUtc, endUtc, cancellationToken))
                 return Result<PolicyDto>.Conflict("Building already has an overlapping active policy.");

            var policy = Policy.CreateDraft(
                clientId: request.ClientId,
                buildingId: request.BuildingId,
                brokerId: request.BrokerId,
                currencyId: request.CurrencyId,
                startDate: startUtc,
                endDate: endUtc,
                basePremium: request.BasePremium);

            var pricing = await _policyPricingService.CalculateAsync(policy, building, cancellationToken);

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                await _unitOfWork.Policies.AddAsync(policy, cancellationToken);

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
                    EstimatedFinalPremium = pricing.FinalPremium,
                    CancellationEffectiveDate = policy.CancellationEffectiveDate
                };

                return Result<PolicyDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during policy creation for BrokerId = {BrokerId}", request.BrokerId);
                return Result<PolicyDto>.Failure("Unexpected error during policy creation.", ErrorType.Generic);
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                    }
                    catch (Exception finalRbEx)
                    {
                        _logger.LogWarning(finalRbEx, "Final rollback attempt failed during buildind creation for ClientId = {ClientId}", request.ClientId);
                    }
                }
            }
        }
    }
}