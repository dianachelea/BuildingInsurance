using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities;
using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties;
using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Brokers.Geography
{
    [Route("api/brokers")]
    [ApiController]
    public sealed class GeographyController : BaseController
    {
        private readonly IMediator _mediator;

        public GeographyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListCountriesQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 10
            }, ct);
            return ToActionResult(result);
        }

        [HttpGet("countries/{countryId:guid}/counties")]
        public async Task<IActionResult> GetCountiesByCountry([FromRoute] Guid countryId, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListCountiesByCountryQuery
            {
                CountryId = countryId,
                Page = page ?? 1,
                PageSize = pageSize ?? 10
            }, ct);
            return ToActionResult(result);
        }

        [HttpGet("counties/{countyId:guid}/cities")]
        public async Task<IActionResult> GetCitiesByCounty([FromRoute] Guid countyId, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListCitiesByCountyQuery
            {
                CountyId = countyId,
                Page = page ?? 1,
                PageSize = pageSize ?? 10
            }, ct);
            return ToActionResult(result);
        }
    }
}