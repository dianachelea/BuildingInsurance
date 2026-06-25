using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection
{
    public interface IPolicyReportStrategySelector
    {
        IPolicyReportStrategy Select(ReportDimension dimension);
    }
}