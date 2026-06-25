using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Mapping
{
    public static class PolicyStatusContractMapping
    {
        public static PolicyStatus? MapToDomainPolicyStatusOptional(this PolicyStatusContract? type)
        {
            if (type is null)
                return null;

            if (type == PolicyStatusContract.Active)
                return PolicyStatus.Active;
            else if (type == PolicyStatusContract.Expired)
                return PolicyStatus.Expired;
            else if (type == PolicyStatusContract.Cancelled)
                return PolicyStatus.Cancelled;
            else if (type == PolicyStatusContract.Draft)
                return PolicyStatus.Draft;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported policy status.");
        }

        public static PolicyStatus MapToDomainPolicyStatus(this PolicyStatusContract type)
        {
            if (type == PolicyStatusContract.Active)
                return PolicyStatus.Active;
            else if (type == PolicyStatusContract.Expired)
                return PolicyStatus.Expired;
            else if (type == PolicyStatusContract.Cancelled)
                return PolicyStatus.Cancelled;
            else if (type == PolicyStatusContract.Draft)
                return PolicyStatus.Draft;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported policy status.");
        }
    }
}