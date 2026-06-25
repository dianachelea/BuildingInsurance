using BuildingInsurance.API.Contracts.Administrators.FeeConfigurations;
using BuildingInsurance.API.Mapping;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.CreateFeeConfiguration;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.UpdateFeeConfiguration;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.GetFeeConfigurationById;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Administrators.Metadata
{
    [Route("api/admin/fees")]
    [ApiController]
    public class FeeConfigurationController : BaseController
    {
        private readonly IMediator _mediator;
        public FeeConfigurationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeeConfiguration([FromBody] CreateFeeConfigurationRequestDto request, CancellationToken ct)
        {
            var command = new CreateFeeConfigurationCommand
            {
                Name = request.Name,
                FeeType = request.FeeType.MapToContractFeeType(),
                FeePercentage = request.FeePercentage,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsActive = request.IsActive,
                RiskIndicators = request.RiskIndicators.MapToContractRiskIndicators()
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { feeConfigurationId = result.Value.Id }, result);
        }

        [HttpGet("{feeConfigurationId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid feeConfigurationId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetFeeConfigurationByIdQuery(feeConfigurationId), ct);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeeConfigurations([FromQuery] string? name, [FromQuery] FeeTypeRequestDto? type, [FromQuery] bool? isActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct = default)
        {
            var result = await _mediator.Send(
                new ListFeeConfigurationsQuery { 
                    Name = name,
                    Type = type.MapToContractFeeTypeOptional(),
                    IsActive = isActive,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10},
                ct);

            return ToActionResult(result);
        }

        [HttpPut("{feeConfigurationId:guid}")]
        public async Task<IActionResult> UpdateFeeConfiguration(Guid feeConfigurationId, [FromBody] UpdateFeeConfigurationRequestDto request, CancellationToken ct)
        {
            var command = new UpdateFeeConfigurationCommand
            {
                Id = feeConfigurationId,
                Name = request.Name,
                FeeType = request.FeeType.MapToContractFeeType(),
                FeePercentage = request.FeePercentage,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsActive = request.IsActive,
                RiskIndicators = request.RiskIndicators.MapToContractRiskIndicators()
            };

            var result = await _mediator.Send(command, ct);
            return ToActionResult(result);
        }
    }
}