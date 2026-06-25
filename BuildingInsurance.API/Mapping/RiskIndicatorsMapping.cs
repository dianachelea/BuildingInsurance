using BuildingInsurance.API.Contracts.Brokers.Buildings;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.API.Mapping
{
    public static class RiskIndicatorsMapping
    {
        public static RiskIndicatorsContract MapToContractRiskIndicators(this RiskIndicatorsRequestDto risk)
        {
            return (RiskIndicatorsContract)(int)risk;
        }
    }
}