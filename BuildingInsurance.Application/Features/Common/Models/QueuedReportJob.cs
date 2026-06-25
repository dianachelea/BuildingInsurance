using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;

namespace BuildingInsurance.Application.Features.Common.Models
{
    public sealed class QueuedReportJob
    {
        public Guid JobId { get; init; }
        public ReportDimension Dimension { get; init; }
        public PolicyReportFilters Filters { get; init; } = default!;
    }
}