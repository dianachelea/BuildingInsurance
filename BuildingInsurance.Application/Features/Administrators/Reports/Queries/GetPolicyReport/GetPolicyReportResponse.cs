using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport
{
    public sealed record GetPolicyReportResponse : PaginatedResult<PolicyReportRowDto>
    {
        public GetPolicyReportResponse(IReadOnlyList<PolicyReportRowDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}