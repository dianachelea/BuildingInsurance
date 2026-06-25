using BuildingInsurance.API.Contracts.Administrators.Reports.Jobs;
using BuildingInsurance.API.Mapping;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Commands.CreateReportJob;
using BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Queries.GetReportJobResultQuery;
using BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Queries.GetReportJobStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BuildingInsurance.API.Controllers.Administrators.Reports
{
    [ApiController]
    [Route("api/admin/reports/jobs")]
    public sealed class ReportJobsController : BaseController
    {
        private readonly IMediator _mediator;

        public ReportJobsController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReportJobRequestDto request, CancellationToken ct)
        {
            var filters = new PolicyReportFilters(
                request.Filters.From,
                request.Filters.To,
                request.Filters.Status.MapToContractPolicyStatusOptional(),
                request.Filters.Currency,
                request.Filters.BuildingType.MapToContractBuildingTypeOptional()
            );

            var command = new CreateReportJobCommand
            {
                Dimension = request.Dimension.MapToApplicationReportDimension(),
                Filters = filters
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsFailure)
                return ToActionResult(result);

            var jobId = result.Value;
            var response = new CreateReportJobResponseDto(jobId);

            return AcceptedAtAction(nameof(GetStatus), new { jobId }, response);
        }

        [HttpGet("{jobId:guid}")]
        public async Task<IActionResult> GetStatus([FromRoute] Guid jobId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetReportJobStatusQuery(jobId), ct);
            return ToActionResult(result);
        }

        [HttpGet("{jobId:guid}/result")]
        public async Task<IActionResult> GetResult([FromRoute] Guid jobId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetReportJobResultQuery(jobId), ct);
            return ToActionResult(result);
        }
    }
}