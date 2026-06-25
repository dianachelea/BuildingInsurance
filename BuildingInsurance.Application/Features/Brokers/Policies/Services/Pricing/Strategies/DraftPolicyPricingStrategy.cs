using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Extensions;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Services.Pricing.Strategies
{
    public sealed class DraftPolicyPricingStrategy : IPolicyPricingStrategy
    {
        private readonly IUnitOfWork _unitOfWork;

        public DraftPolicyPricingStrategy(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool CanHandle(PolicyStatus status) => status == PolicyStatus.Draft;

        public async Task<PolicyPricingResult> CalculateAsync(Policy policy, Building building, CancellationToken ct)
        {
            var effectiveAtUtc = policy.StartDate.ToUtc();

            var basePremium = policy.BasePremium;
            if (basePremium <= 0m)
                throw new ArgumentOutOfRangeException(nameof(policy), "Base premium must be positive.");

            var city = await _unitOfWork.Cities.GetByIdAsync(building.CityId, ct) ?? throw new InvalidOperationException("City not found.");
            var county = await _unitOfWork.Counties.GetByIdAsync(city.CountyId, ct) ?? throw new InvalidOperationException("County not found.");
            var countryId = county.CountryId;

            var feeConfigs = await _unitOfWork.FeeConfigurations.GetAllAsync(ct);

            var fees = feeConfigs
                .Where(f => f.IsEffectiveAt(effectiveAtUtc))
                .Where(f => f.FeeType != FeeType.RiskAdjustment
                    || (building.RiskIndicators & f.RiskIndicators) == f.RiskIndicators)
                .Select(f => new AppliedFeeSnapshot(f.Id, f.Name, f.FeePercentage))
                .ToList();

            var risks = new List<AppliedRiskSnapshot>();

            var countryRisks = await _unitOfWork.RiskFactorConfigurations.GetActiveForAsync(countryId, null, ct);
            risks.AddRange(countryRisks.Select(r => new AppliedRiskSnapshot(r.Id, r.Level, r.ReferenceId, r.BuildingType, r.AdjustmentPercentage)));

            var countyRisks = await _unitOfWork.RiskFactorConfigurations.GetActiveForAsync(county.Id, null, ct);
            risks.AddRange(countyRisks.Select(r => new AppliedRiskSnapshot(r.Id, r.Level, r.ReferenceId, r.BuildingType, r.AdjustmentPercentage)));

            var cityRisks = await _unitOfWork.RiskFactorConfigurations.GetActiveForAsync(city.Id, null, ct);
            risks.AddRange(cityRisks.Select(r => new AppliedRiskSnapshot(r.Id, r.Level, r.ReferenceId, r.BuildingType, r.AdjustmentPercentage)));

            var buildingTypeRisks = await _unitOfWork.RiskFactorConfigurations.GetActiveForAsync(null, building.Type, ct);
            risks.AddRange(buildingTypeRisks.Select(r => new AppliedRiskSnapshot(r.Id, r.Level, r.ReferenceId, r.BuildingType, r.AdjustmentPercentage)));

            risks = risks
                .GroupBy(r => r.RiskFactorConfigurationId)
                .Select(g => g.First())
                .ToList();

            var feeSum = fees.Sum(x => x.Percentage);
            var riskSum = risks.Sum(x => x.AdjustmentPercentage);

            var finalPremium = basePremium * (1m + feeSum + riskSum);
            if (finalPremium <= 0m)
                throw new InvalidOperationException("Final premium cannot be <= 0 after adjustments.");

            return new PolicyPricingResult(finalPremium, fees, risks);
        }
    }
}
