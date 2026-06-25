namespace BuildingInsurance.API.Contracts.Administrators.Reports.Jobs
{
    public sealed record ReportJobStatusResponseDto(
        Guid JobId,
        string Status,
        int Progress,
        string? Error,
        Guid? ResultId
        );
}