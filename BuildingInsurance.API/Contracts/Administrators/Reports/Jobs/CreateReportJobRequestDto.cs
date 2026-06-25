namespace BuildingInsurance.API.Contracts.Administrators.Reports.Jobs
{
    public sealed record CreateReportJobRequestDto(
        ReportDimensionRequestDto Dimension,
        PolicyReportQueryRequestDto Filters);
}