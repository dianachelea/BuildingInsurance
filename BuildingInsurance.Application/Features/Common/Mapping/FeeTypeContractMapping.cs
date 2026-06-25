using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Mapping
{
    public static class FeeTypeContractMapping
    {
        public static FeeType MapToDomainFeeType(this FeeTypeContract type)
        {
            if (type == FeeTypeContract.BrokerCommission)
                return FeeType.BrokerCommission;
            else if (type == FeeTypeContract.AdminFee)
                return FeeType.AdminFee;
            else if (type == FeeTypeContract.RiskAdjustment)
                return FeeType.RiskAdjustment;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported fee type.");
        }

        public static FeeType? MapToDomainFeeTypeOptional(this FeeTypeContract? type)
        {
            if (type is null)
                return null;

            if (type == FeeTypeContract.BrokerCommission)
                return FeeType.BrokerCommission;
            else if (type == FeeTypeContract.AdminFee)
                return FeeType.AdminFee;
            else if (type == FeeTypeContract.RiskAdjustment)
                return FeeType.RiskAdjustment;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported fee type.");
        }
    }
}