using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Mapping
{
    public static class ClientTypeContractMapping
    {
        public static ClientType MapToDomainClientType(this ClientTypeContract type)
        {
            if (type == ClientTypeContract.Individual)
                return ClientType.Individual;
            else if(type == ClientTypeContract.Company)
                return ClientType.Company;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported client type.");
        }
    }
}