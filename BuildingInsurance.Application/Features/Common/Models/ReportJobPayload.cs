using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;

namespace BuildingInsurance.Application.Features.Common.Models
{
    public sealed class ReportJobPayload
    {
        public ReportDimension Dimension { get; set; }
        public PolicyReportFilters Filters { get; set; } = default!;
    }
}