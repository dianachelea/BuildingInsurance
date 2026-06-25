using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Commands.CreateReportJob
{
    public sealed class CreateReportJobCommand : ICommand<Result<Guid>>
    {
        public ReportDimension Dimension { get; set; }
        public PolicyReportFilters Filters { get; set; }
    }
}