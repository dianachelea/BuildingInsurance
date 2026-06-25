using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Queries.GetReportJobResultQuery
{
    public sealed record GetReportJobResultQuery(Guid JobId) : IRequest<Result<List<PolicyReportRowDto>>>;
}