using BuildingInsurance.API.Contracts.Brokers.Buildings;
using BuildingInsurance.API.Mapping;
using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.CreateBuilding;
using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.UpdateBuilding;
using BuildingInsurance.Application.Features.Brokers.Buildings.Queries.GetBuildingById;
using BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient;
using BuildingInsurance.Application.Features.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Brokers.Buildings
{
    [Route("api/brokers")]
    [ApiController]
    public class BuildingsController : BaseController
    {
        private readonly IMediator _mediator;

        public BuildingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("clients/{clientId:guid}/buildings")]
        public async Task<IActionResult> CreateBuilding([FromRoute] Guid clientId, [FromBody] CreateBuildingRequestDto request, CancellationToken ct)
        {
            var command = new CreateBuildingCommand
            {
                ClientId = clientId,
                CityId = request.CityId,
                Street = request.Address.Street,
                Number = request.Address.Number,
                ConstructionYear = request.ConstructionYear,
                Type = request.Type.MapToContractBuildingType(),
                NumberOfFloors = request.NumberOfFloors,
                SurfaceArea = request.SurfaceArea,
                InsuredValue = request.InsuredValue,
                RiskIndicators = request.RiskIndicators.MapToContractRiskIndicators()
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { buildingId = result.Value.Id }, result);
        }

        [HttpPut("buildings/{buildingId:guid}")]
        public async Task<IActionResult> UpdateBuilding(Guid buildingId, [FromBody] UpdateBuildingRequestDto request, CancellationToken ct)
        {
            var command = new UpdateBuildingCommand
            {
                BuildingId = buildingId,
                CityId = request.CityId,
                Address = new AddressDto
                {
                    Street = request.Address.Street,
                    Number = request.Address.Number
                },
                ConstructionYear = request.ConstructionYear,
                Type = request.Type.MapToContractBuildingType(),
                NumberOfFloors = request.NumberOfFloors,
                SurfaceArea = request.SurfaceArea,
                InsuredValue = request.InsuredValue,
                RiskIndicators = request.RiskIndicators.MapToContractRiskIndicators()
            };

            var result = await _mediator.Send(command, ct);
            return ToActionResult(result);
        }

        [HttpGet("buildings/{buildingId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid buildingId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetBuildingByIdQuery(buildingId), ct);

            return ToActionResult(result);
        }

        [HttpGet("clients/{clientId:guid}/buildings")]
        public async Task<IActionResult> GetBuildingsByClientId([FromRoute] Guid clientId, [FromQuery] int? page, [FromQuery] int? pagesize, CancellationToken ct)
        {
            var result = await _mediator.Send(new ListBuildingsByClientQuery { ClientId = clientId, Page = page ?? 1, PageSize = pagesize ?? 10}, ct);
            
            return ToActionResult(result);
        }
    }
}