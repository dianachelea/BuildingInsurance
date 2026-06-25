using BuildingInsurance.API.Contracts.Brokers.Policies;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.API.Mapping
{
    public static class PolicyStatusMapping
    {
        public static PolicyStatusContract? MapToContractPolicyStatusOptional(this PolicyStatusRequestDto? type)
        {
            if (type is null)
                return null;

            if (type == PolicyStatusRequestDto.Cancelled)
                return PolicyStatusContract.Cancelled;
            else if (type == PolicyStatusRequestDto.Draft)
                return PolicyStatusContract.Draft;
            else if (type == PolicyStatusRequestDto.Active)
                return PolicyStatusContract.Active;
            else if (type == PolicyStatusRequestDto.Expired)
                return PolicyStatusContract.Expired;
            else 
                throw new ArgumentOutOfRangeException(nameof(type), $"Not expected policy status value: {type}");
        }

        public static PolicyStatusContract MapToContractPolicyStatus(this PolicyStatusRequestDto type)
        {
            if (type == PolicyStatusRequestDto.Cancelled)
                return PolicyStatusContract.Cancelled;
            else if (type == PolicyStatusRequestDto.Draft)
                return PolicyStatusContract.Draft;
            else if (type == PolicyStatusRequestDto.Active)
                return PolicyStatusContract.Active;
            else if (type == PolicyStatusRequestDto.Expired)
                return PolicyStatusContract.Expired;
            else
                throw new ArgumentOutOfRangeException(nameof(type), $"Not expected policy status value: {type}");
        }
    }
}