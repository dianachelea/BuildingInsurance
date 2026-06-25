using BuildingInsurance.API.Contracts.Brokers.Buildings;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.API.Mapping
{
    public static class BuildingTypeMapping
    {
        public static BuildingTypeContract MapToContractBuildingType(this BuildingTypeRequestDto type)
        {
            if (type == BuildingTypeRequestDto.Office)
                return BuildingTypeContract.Office;
            else if (type == BuildingTypeRequestDto.Industrial)
                return BuildingTypeContract.Industrial;
            else if (type == BuildingTypeRequestDto.Residential)
                return BuildingTypeContract.Residential;
            else
                throw new ArgumentOutOfRangeException(nameof(type), $"Not expected building type value: {type}");
        }
        
        public static BuildingTypeContract? MapToContractBuildingTypeOptional(this BuildingTypeRequestDto? type)
        {
            if (type is null)
                return null;

            if (type == BuildingTypeRequestDto.Office)
                return BuildingTypeContract.Office;
            else if (type == BuildingTypeRequestDto.Industrial)
                return BuildingTypeContract.Industrial;
            else if (type == BuildingTypeRequestDto.Residential)
                return BuildingTypeContract.Residential;
            else
                throw new ArgumentOutOfRangeException(nameof(type), $"Not expected building type value: {type}");
        }
    }
}