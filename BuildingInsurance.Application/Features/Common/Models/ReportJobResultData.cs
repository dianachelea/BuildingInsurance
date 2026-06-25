using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;

namespace BuildingInsurance.Application.Features.Common.Models
{
    public sealed record ReportJobResultData(
        Guid JobId,
        bool IsReady,
        bool IsFailed,
        string? Error,
        List<PolicyReportRowDto>? Rows);
}