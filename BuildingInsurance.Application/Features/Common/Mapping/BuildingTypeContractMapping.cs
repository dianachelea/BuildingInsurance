using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Mapping
{
    public static class BuildingTypeContractMapping
    {
        public static BuildingType MapToDomainBuildingType(this BuildingTypeContract type)
        {
            if (type == BuildingTypeContract.Office)
                return BuildingType.Office;
            else if (type == BuildingTypeContract.Industrial)
                return BuildingType.Industrial;
            else if (type == BuildingTypeContract.Residential)
                return BuildingType.Residential;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported building type.");
        }

        public static BuildingType? MapToDomainBuildingTypeOptional(this BuildingTypeContract? type)
        {
            if (type is null)
                return null;

            if (type == BuildingTypeContract.Office)
                return BuildingType.Office;
            else if (type == BuildingTypeContract.Industrial)
                return BuildingType.Industrial;
            else if (type == BuildingTypeContract.Residential)
                return BuildingType.Residential;
            else
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported building type.");
        }
    }
}