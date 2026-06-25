using BuildingInsurance.API.Contracts.Administrators.Brokers;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.ActivateBroker;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.CreateBroker;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.DeactivateBroker;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.UpdateBroker;
using BuildingInsurance.Application.Features.Administrators.Brokers.Queries.GetBrokerById;
using BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Administrators.Broker
{
    [Route("api/admin/brokers")]
    [ApiController]
    public sealed class BrokerController : BaseController
    {
        private readonly IMediator _mediator;

        public BrokerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBroker([FromBody] CreateBrokerRequestDto request, CancellationToken ct)
        {
            var command = new CreateBrokerCommand
            {
                BrokerCode = request.BrokerCode,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                CommissionPercentage = request.CommissionPercentage
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { brokerId = result.Value.Id }, result);
        }

        [HttpGet("{brokerId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid brokerId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetBrokerByIdQuery(brokerId), ct);
            return ToActionResult(result);
        }

        [HttpPost("{brokerId:guid}/activate")]
        public async Task<IActionResult> Activate([FromRoute] Guid brokerId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ActivateBrokerCommand(brokerId), ct);
            return ToActionResult(result);
        }

        [HttpPost("{brokerId:guid}/deactivate")]
        public async Task<IActionResult> Deactivate([FromRoute] Guid brokerId, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeactivateBrokerCommand(brokerId), ct);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBrokers([FromQuery] string? name, [FromQuery] bool? isActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new ListBrokersQuery {
                    Name = name,
                    IsActive = isActive,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10},
                ct);

            return ToActionResult(result);
        }

        [HttpPut("{brokerId:guid}")]
        public async Task<IActionResult> UpdateBroker([FromRoute] Guid brokerId, [FromBody] UpdateBrokerRequestDto request, CancellationToken ct)
        {
            var command = new UpdateBrokerCommand
            {
                Id = brokerId,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                CommissionPercentage = request.CommissionPercentage
            };

            var result = await _mediator.Send(command, ct);
            return ToActionResult(result);
        }
    }
}