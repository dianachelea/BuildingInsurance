using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Mapping
{
    public static class RiskFactorLevelContractMapping
    {
        public static RiskFactorLevel MapToDomainRiskFactorLevel(this RiskFactorLevelContract type)
        {
            if (type == RiskFactorLevelContract.BuildingType)
                return RiskFactorLevel.BuildingType;
            else if (type == RiskFactorLevelContract.Country)
                return RiskFactorLevel.Country;
            else if (type == RiskFactorLevelContract.County)
                return RiskFactorLevel.County;
            else if (type == RiskFactorLevelContract.City)
                return RiskFactorLevel.City;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported risk factor level.");
        }

        public static RiskFactorLevel? MapToDomainRiskFactorLevelOptional(this RiskFactorLevelContract? type)
        {
            if (type is null)
                return null;

            if (type == RiskFactorLevelContract.BuildingType)
                return RiskFactorLevel.BuildingType;
            else if (type == RiskFactorLevelContract.Country)
                return RiskFactorLevel.Country;
            else if (type == RiskFactorLevelContract.County)
                return RiskFactorLevel.County;
            else if (type == RiskFactorLevelContract.City)
                return RiskFactorLevel.City;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported risk factor level.");
        }
    }
}