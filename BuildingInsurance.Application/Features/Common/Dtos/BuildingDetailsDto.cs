using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class BuildingDetailsDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public string Street { get; set; } = null!;
        public string Number { get; set; } = null!;
        public string City { get; set; } = null!;
        public string County { get; set; } = null!;
        public string Country { get; set; } = null!;
        public int ConstructionYear { get; set; }
        public BuildingType Type { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
    }
}