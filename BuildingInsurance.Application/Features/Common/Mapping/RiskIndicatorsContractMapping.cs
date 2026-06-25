using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Mapping
{
    public static class RiskIndicatorsContractMapping
    {
        public static RiskIndicators MapToDomainRiskIndicators(this RiskIndicatorsContract risk)
        {
            return (RiskIndicators)(int)risk;
        }
    }
}