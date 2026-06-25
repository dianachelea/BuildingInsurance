using BuildingInsurance.API.Contracts.Brokers.Clients;

namespace BuildingInsurance.API.Contracts.Brokers.Buildings
{
    public sealed class UpdateBuildingRequestDto
    {
        public Guid CityId { get; set; }
        public AddressRequestDto Address { get; set; } = null!;
        public int ConstructionYear { get; set; }
        public BuildingTypeRequestDto Type { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
        public RiskIndicatorsRequestDto RiskIndicators { get; set; }
    }
}