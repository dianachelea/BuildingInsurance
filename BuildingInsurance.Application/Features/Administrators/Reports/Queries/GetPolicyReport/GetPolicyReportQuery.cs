using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport
{
    public sealed record GetPolicyReportQuery(ReportDimension Dimension, PolicyReportFilters Filters) : PaginatedQuery, IRequest<Result<GetPolicyReportResponse>>;
}