using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Queries.GetReportJobStatus
{
    public sealed record GetReportJobStatusQuery(Guid JobId) : IRequest<Result<ReportJobStatusData>>;
}