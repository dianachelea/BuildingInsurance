using BuildingInsurance.API.Contracts.Administrators.RiskFactorConfigurations;
using BuildingInsurance.API.Mapping;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.CreateRiskFactorConfiguration;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.UpdateRiskFactorConfiguration;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.GetRiskFactorConfigurationById;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Administrators.Metadata
{
    [ApiController]
    [Route("api/admin/risk-factors")]
    public sealed class RiskFactorConfigurationsController : BaseController
    {
        private readonly IMediator _mediator;

        public RiskFactorConfigurationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRiskFactorConfigurationRequestDto request, CancellationToken ct)
        {
            var command = new CreateRiskFactorConfigurationCommand
            {
                Level = request.Level.MapToContractRiskFactorLevel(),
                ReferenceId = request.ReferenceId,
                BuildingType = request.BuildingType.MapToContractBuildingTypeOptional(),
                AdjustmentPercentage = request.AdjustmentPercentage,
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { riskFactorConfigurationId = result.Value.Id }, result);
        }

        [HttpGet("{riskFactorConfigurationId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid riskFactorConfigurationId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetRiskFactorConfigurationByIdQuery(riskFactorConfigurationId), ct);
            return ToActionResult(result);
        }

        [HttpPut("{riskFactorConfigurationId:guid}")]
        public async Task<IActionResult> UpdateRiskFactorConfiguration(Guid riskFactorConfigurationId, [FromBody] UpdateRiskFactorConfigurationRequestDto request, CancellationToken ct)
        {
            var command = new UpdateRiskFactorConfigurationCommand
            {
                Id = riskFactorConfigurationId,
                Level = request.Level.MapToContractRiskFactorLevel(),
                ReferenceId = request.ReferenceId,
                BuildingType = request.BuildingType.MapToContractBuildingTypeOptional(),
                AdjustmentPercentage = request.AdjustmentPercentage,
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command, ct);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRiskFactorConfigurations([FromQuery] RiskFactorLevelRequestDto? level, [FromQuery] Guid? referenceId, [FromQuery] bool? isActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct = default)
        {
            var result = await _mediator.Send(
                new ListRiskFactorConfigurationsQuery {
                    Level = level.MapToContractRiskFactorLevelOptional(),
                    ReferenceId = referenceId,
                    IsActive = isActive,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10},
                ct);

            return ToActionResult(result);
        }
    }
}