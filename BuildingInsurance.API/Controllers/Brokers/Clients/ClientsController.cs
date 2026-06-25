using BuildingInsurance.API.Contracts.Brokers.Clients;
using BuildingInsurance.API.Mapping;
using BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient;
using BuildingInsurance.Application.Features.Brokers.Clients.Commands.UpdateClient;
using BuildingInsurance.Application.Features.Brokers.Clients.Queries.GetClient;
using BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients;
using BuildingInsurance.Application.Features.Common.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Brokers.Clients
{
    [ApiController]
    [Route("api/brokers/clients")]
    public class ClientsController : BaseController
    {
        private readonly IMediator _mediator;

        public ClientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequestDto request, CancellationToken ct)
        {
            var command = new CreateClientCommand
            {
                Type = request.Type.MapToContractClientType(),
                FullName = request.FullName,
                PersonalIdentificationNumber = request.PersonalIdentificationNumber,
                CompanyRegistrationNumber = request.CompanyRegistrationNumber,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address is null
                    ? null
                    : new AddressDto { Street = request.Address.Street, Number = request.Address.Number }
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { clientId = result.Value.Id }, result);
        }

        [HttpPut("{clientId:guid}")]
        public async Task<IActionResult> UpdateClient(Guid clientId, [FromBody] UpdateClientRequestDto request, CancellationToken ct)
        {
            var command = new UpdateClientCommand
            {
                ClientId = clientId,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address is null
                    ? null
                    : new AddressDto
                    {
                        Street = request.Address.Street,
                        Number = request.Address.Number
                    },
                IdentificationNumber = request.IdentificationNumber,
                IdentificationChangeReason = request.IdentificationChangeReason
            };

            var result = await _mediator.Send(command, ct);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? name, [FromQuery] string? identifier, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct = default)
        {
            var result = await _mediator.Send(
                new ListClientsQuery
                {
                    Name = name,
                    Identifier = identifier,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10},
                ct);

            return ToActionResult(result);
        }

        [HttpGet("{clientId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid clientId, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetClientByIdQuery(clientId), ct);

            return ToActionResult(result);
        }
    }
 }