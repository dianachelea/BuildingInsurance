using BuildingInsurance.API.Contracts.Brokers.Clients;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.API.Mapping
{
    public static class ClientTypeMapping
    {
        public static ClientTypeContract MapToContractClientType(this ClientTypeRequestDto type)
        {
            if (type == ClientTypeRequestDto.Individual)
                return ClientTypeContract.Individual;
            else if (type == ClientTypeRequestDto.Company)
                return ClientTypeContract.Company;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported client type.");
        }
    }
}