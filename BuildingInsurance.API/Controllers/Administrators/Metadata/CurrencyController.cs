using BuildingInsurance.API.Contracts.Administrators.Currencies;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.CreateCurrency;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.UpdateCurrency;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.GetCurrencyById;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Administrators.Metadata
{
    [Route("api/admin/currencies")]
    [ApiController]
    public class CurrencyController : BaseController
    {
        private readonly IMediator _mediator;

        public CurrencyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyRequestDto request, CancellationToken ct)
        {
            var command = new CreateCurrencyCommand
            {
                Code = request.Code,
                Name = request.Name,
                ExchangeRateToBase = request.ExchangeRateToBase,
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            return CreatedAtAction(nameof(GetById), new { currencyId = result.Value.Id }, result);
        }

        [HttpPut("{currencyId:guid}")]
        public async Task<IActionResult> UpdateCurrency(Guid currencyId, [FromBody] UpdateCurrencyRequestDto request, CancellationToken ct)
        {
            var command = new UpdateCurrencyCommand
            {
                Id = currencyId,
                Name = request.Name,
                ExchangeRateToBase = request.ExchangeRateToBase,
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command, ct);
            return ToActionResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencies([FromQuery] string? name, [FromQuery] bool? isActive, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct = default)
        {
            var result = await _mediator.Send(
                new ListCurrenciesQuery
                {
                    Name = name,
                    IsActive = isActive,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10
                },
                ct);

            return ToActionResult(result);
        }

        [HttpGet("{currencyId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid currencyId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCurrencyByIdQuery(currencyId), ct);
            return ToActionResult(result);
        }
    }
}