using BuildingInsurance.API.Contracts.Administrators.Reports;
using BuildingInsurance.API.Mapping;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Administrators.Reports
{
    [ApiController]
    [Route("api/admin/reports")]
    public sealed class ReportsController : BaseController
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("policies-by-country")]
        public Task<IActionResult> PoliciesByCountry([FromQuery] PolicyReportQueryRequestDto request, CancellationToken ct)
            => GetPolicyReport(ReportDimension.Country, request, ct);

        [HttpGet("policies-by-county")]
        public Task<IActionResult> PoliciesByCounty([FromQuery] PolicyReportQueryRequestDto request, CancellationToken ct)
            => GetPolicyReport(ReportDimension.County, request, ct);

        [HttpGet("policies-by-city")]
        public Task<IActionResult> PoliciesByCity([FromQuery] PolicyReportQueryRequestDto request, CancellationToken ct)
            => GetPolicyReport(ReportDimension.City, request, ct);

        [HttpGet("policies-by-broker")]
        public Task<IActionResult> PoliciesByBroker([FromQuery] PolicyReportQueryRequestDto request, CancellationToken ct)
            => GetPolicyReport(ReportDimension.Broker, request, ct);

        private async Task<IActionResult> GetPolicyReport(ReportDimension dimension, PolicyReportQueryRequestDto request, CancellationToken ct)
        {
            var filters = new PolicyReportFilters(
                request.From,
                request.To,
                request.Status.MapToContractPolicyStatusOptional(),
                request.Currency,
                request.BuildingType.MapToContractBuildingTypeOptional()
            );

            var page = request.Page ?? 1;
            var pageSize = request.PageSize ?? 10;

            var query = new GetPolicyReportQuery(dimension, filters)
            {
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, ct);
            return ToActionResult(result);
        }
    }
}