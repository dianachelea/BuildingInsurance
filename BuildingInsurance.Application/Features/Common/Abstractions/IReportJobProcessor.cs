using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;

namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface IReportJobProcessor
    {
        Task<List<PolicyReportRowDto>> GenerateAsync(ReportDimension dimension, PolicyReportFilters filters, CancellationToken ct);
    }
}