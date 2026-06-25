using BuildingInsurance.API.Contracts.Administrators.RiskFactorConfigurations;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.API.Mapping
{
    public static class RiskFactorLevelMapping
    {
        public static RiskFactorLevelContract MapToContractRiskFactorLevel(this RiskFactorLevelRequestDto type)
        {
            if (type == RiskFactorLevelRequestDto.Country)
                return RiskFactorLevelContract.Country;
            else if (type == RiskFactorLevelRequestDto.County)
                return RiskFactorLevelContract.County;
            else if (type == RiskFactorLevelRequestDto.City)
                return RiskFactorLevelContract.City;
            else if (type == RiskFactorLevelRequestDto.BuildingType)
                return RiskFactorLevelContract.BuildingType;
            else
                throw new ArgumentOutOfRangeException(nameof(type), $"Not expected RiskFactorLevel type value: {type}");
        }

        public static RiskFactorLevelContract? MapToContractRiskFactorLevelOptional(this RiskFactorLevelRequestDto? type)
        {
            if (type is null)
                return null;

            if (type == RiskFactorLevelRequestDto.Country)
                return RiskFactorLevelContract.Country;
            else if (type == RiskFactorLevelRequestDto.County)
                return RiskFactorLevelContract.County;
            else if (type == RiskFactorLevelRequestDto.City)
                return RiskFactorLevelContract.City;
            else if (type == RiskFactorLevelRequestDto.BuildingType)
                return RiskFactorLevelContract.BuildingType;
            else
                throw new ArgumentOutOfRangeException(nameof(type), $"Not expected RiskFactorLevel type value: {type}");
        }
    }
}