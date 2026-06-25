namespace BuildingInsurance.Application.Features.Common.Models
{
    public sealed record ReportJobStatusData(
        Guid JobId,
        string Status,
        int Progress,
        string? Error,
        Guid? ResultId);
}