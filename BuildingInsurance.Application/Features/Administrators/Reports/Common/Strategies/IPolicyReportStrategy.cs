using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies
{
    public interface IPolicyReportStrategy
    {
        bool CanHandle(ReportDimension dimension);

        Task<List<PolicyReportRowDto>> GenerateReportAsync(DateTime from, 
            DateTime to, 
            PolicyStatus status, 
            string currencyCode, 
            BuildingType? buildingType, 
            CancellationToken ct);
    }
}