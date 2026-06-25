using BuildingInsurance.API.Contracts.Brokers.Policies;
using BuildingInsurance.API.Mapping;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CancelPolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries.GetPolicyById;
using BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Brokers.Policies
{
    [ApiController]
    [Route("api/brokers/policies")]
    public sealed class PoliciesController : BaseController
    {
        private readonly IMediator _mediator;

        public PoliciesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDraft([FromBody] CreateDraftPolicyRequestDto request, CancellationToken ct)
        {
            var command = new CreateDraftPolicyCommand
            {
                ClientId = request.ClientId,
                BuildingId = request.BuildingId,
                CurrencyId = request.CurrencyId,
                BrokerId = request.BrokerId,
                BasePremium = request.BasePremium,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { policyId = result.Value.Id }, result);
        }

        [HttpGet("{policyId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid policyId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPolicyByIdQuery(policyId), ct);

            return ToActionResult(result);
        }

        [HttpPost("{policyId:guid}/activate")]
        public async Task<IActionResult> Activate([FromRoute] Guid policyId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ActivatePolicyCommand(policyId), ct);

            return ToActionResult(result);
        }

        [HttpPost("{policyId:guid}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] Guid policyId, [FromBody] CancelPolicyRequestDto request, CancellationToken ct)
        {
            var command = new CancelPolicyCommand
            {
                PolicyId = policyId,
                Reason = request.Reason,
                CancellationEffectiveDate = request.CancellationEffectiveDate
            };

            var result = await _mediator.Send(command, ct);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] Guid? clientId, [FromQuery] Guid? brokerId, [FromQuery] PolicyStatusRequestDto? status, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new ListPoliciesQuery{
                    ClientId = clientId,
                    BrokerId = brokerId,
                    Status = status.MapToContractPolicyStatusOptional(),
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10},
                ct);

            return ToActionResult(result);
        }
    }
}