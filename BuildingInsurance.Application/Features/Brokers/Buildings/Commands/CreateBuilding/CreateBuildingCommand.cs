using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Commands.CreateBuilding
{
    public sealed class CreateBuildingCommand : ICommand<Result<BuildingDto>>
    {
        public Guid ClientId { get; set; }
        public Guid CityId { get; set; }
        public string Street { get; set; } = null!;
        public string Number { get; set; } = null!;
        public int ConstructionYear { get; set; }
        public BuildingTypeContract Type { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
        public RiskIndicatorsContract RiskIndicators { get; set; }
    }
}