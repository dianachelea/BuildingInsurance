using BuildingInsurance.API.Contracts.Administrators.FeeConfigurations;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.API.Mapping
{
    public static class FeeTypeMapping
    {
        public static FeeTypeContract MapToContractFeeType(this FeeTypeRequestDto type)
        {
            if (type == FeeTypeRequestDto.BrokerCommission)
                return FeeTypeContract.BrokerCommission;
            else if (type == FeeTypeRequestDto.AdminFee)
                return FeeTypeContract.AdminFee;
            else if (type == FeeTypeRequestDto.RiskAdjustment)
                return FeeTypeContract.RiskAdjustment;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported fee type.");
        }

        public static FeeTypeContract? MapToContractFeeTypeOptional(this FeeTypeRequestDto? type)
        {
            if (type is null)
                return null;

            if (type == FeeTypeRequestDto.BrokerCommission)
                return FeeTypeContract.BrokerCommission;
            else if (type == FeeTypeRequestDto.AdminFee)
                return FeeTypeContract.AdminFee;
            else if (type == FeeTypeRequestDto.RiskAdjustment)
                return FeeTypeContract.RiskAdjustment;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported fee type.");
        }
    }
}