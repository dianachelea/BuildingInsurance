using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy
{
    public sealed class ActivatePolicyCommandHandler : IRequestHandler<ActivatePolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly ILogger<ActivatePolicyCommandHandler> _logger;
        private readonly IBrokerVerifier _brokerVerifier;
        private readonly IPolicyPricingService _policyPricingService;
        private readonly IClientBuildingVerifier _clientBuildingVerifier;
        private readonly ICurrencyVerifier _currencyVerifier;

        public ActivatePolicyCommandHandler(IUnitOfWork unitOfWork, IClock clock, ILogger<ActivatePolicyCommandHandler> logger, IBrokerVerifier brokerVerifier, IPolicyPricingService policyPricingService, IClientBuildingVerifier clientBuildingVerifier, ICurrencyVerifier currencyVerifier)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _logger = logger;
            _brokerVerifier = brokerVerifier;
            _policyPricingService = policyPricingService;
            _clientBuildingVerifier = clientBuildingVerifier;
            _currencyVerifier = currencyVerifier;
        }

        public async Task<Result<PolicyDto>> Handle(ActivatePolicyCommand request, CancellationToken cancellationToken)
        {
            var policy = await _unitOfWork.Policies.GetByIdAsync(request.PolicyId, cancellationToken);
            if (policy is null)
                return Result<PolicyDto>.Failure($"Policy with ID {request.PolicyId} not found.", ErrorType.NotFound);
            
            if (policy.PolicyStatus == PolicyStatus.Active)
            {
                var dtoAlreadyActive = new PolicyDto
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

                return Result<PolicyDto>.Success(dtoAlreadyActive);
            }

            var validation = await ValidateBrokerCurrencyBuilding(policy, cancellationToken);
            if (!validation.IsSuccess)
                return Result<PolicyDto>.Failure(validation.Error, validation.ErrorType);

            var pricing = validation.Value.Pricing;
            var currency = validation.Value.Currency;

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                var nowUtc = _clock.UtcNow;

                policy.SetFinalPremium(pricing.FinalPremium);

                if (currency.ExchangeRateToBase <= 0m)
                    return Result<PolicyDto>.Failure("Currency exchange rate to base must be positive.", ErrorType.Validation);

                var finalPremiumInBase = pricing.FinalPremium * currency.ExchangeRateToBase;
                finalPremiumInBase = Math.Round(finalPremiumInBase, 2, MidpointRounding.AwayFromZero);

                policy.SetFinalPremiumInBaseCurrency(finalPremiumInBase);

                policy.Activate(nowUtc);

                var appliedFees = pricing.Fees.Select(f =>
                    new PolicyAppliedFee(
                        policyId: policy.Id,
                        feeConfigurationId: f.FeeConfigurationId,
                        feeName: f.FeeName,
                        percentage: f.Percentage,
                        appliedAtUtc: nowUtc)).ToList();

                var appliedRisks = pricing.Risks.Select(r =>
                    new PolicyAppliedRiskFactor(
                        policyId: policy.Id,
                        riskFactorConfigurationId: r.RiskFactorConfigurationId,
                        level: r.Level,
                        referenceId: r.ReferenceId,
                        buildingType: r.BuildingType,
                        adjustmentPercentage: r.AdjustmentPercentage,
                        appliedAtUtc: nowUtc)).ToList();

                await _unitOfWork.Policies.ReplaceAppliedPricingAsync(policy, appliedFees, appliedRisks, cancellationToken);

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
                    EstimatedFinalPremium = policy.FinalPremium,
                    FinalPremiumInBaseCurrency = policy.FinalPremiumInBaseCurrency,
                    CancellationEffectiveDate = policy.CancellationEffectiveDate
                };

                return Result<PolicyDto>.Success(dto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error activating policy with ID {PolicyId}", request.PolicyId);

                string publicMessage;

                if (policy.PolicyStatus != PolicyStatus.Draft)
                {
                    publicMessage = "Only Draft policies can be activated.";
                }
                else if (policy.StartDate < _clock.UtcNow)
                {
                    publicMessage = "Start date should not be in the past.";
                }
                else
                {
                    publicMessage = "Policy cannot be activated due to invalid state.";
                }

                return Result<PolicyDto>.Failure(publicMessage, ErrorType.Validation);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error activating policy with ID {PolicyId}", request.PolicyId);
                return Result<PolicyDto>.Failure("Invalid input for policy activation.", ErrorType.Validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating policy with ID {PolicyId}", request.PolicyId);
                return Result<PolicyDto>.Failure("An error occurred while activating the policy.", ErrorType.Generic);
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
                        _logger.LogWarning(finalRbEx, "Final rollback attempt failed during policy activation for PolicyId = {PolicyId}", request.PolicyId);
                    }
                }
            }
        }

        private async Task<Result<(Building Building, Currency Currency, PolicyPricingResult Pricing)>> ValidateBrokerCurrencyBuilding(Domain.Entities.Policies.Policy policy, CancellationToken ct)
        {
            var brokerResult = await _brokerVerifier.GetActiveAsync(policy.BrokerId, ct);
            if (!brokerResult.IsSuccess)
                return Result<(Building, Currency, PolicyPricingResult)>.Failure(brokerResult.Error, brokerResult.ErrorType);

            var currencyResult = await _currencyVerifier.GetActiveAsync(policy.CurrencyId, ct);
            if (!currencyResult.IsSuccess)
                return Result<(Building, Currency, PolicyPricingResult)>.Failure("Policy currency is inactive. Change currency or reactivate it before activation.", ErrorType.Conflict);
            
            var clientBuildingResult = await _clientBuildingVerifier.GetAndVerifyAsync(policy.ClientId, policy.BuildingId, ct);
            if (!clientBuildingResult.IsSuccess)
                return Result<(Building, Currency, PolicyPricingResult)>.Failure(clientBuildingResult.Error, clientBuildingResult.ErrorType);

            var building = clientBuildingResult.Value.Building;
            var currency = currencyResult.Value;

            var pricing = await _policyPricingService.CalculateAsync(policy, building, ct);
            if (pricing is null)
                return Result<(Building, Currency, PolicyPricingResult)>.Failure("Policy pricing must be calculated before activation.", ErrorType.Validation);
            
            return Result<(Building, Currency, PolicyPricingResult)>.Success((building, currency, pricing));
        }
    }
}