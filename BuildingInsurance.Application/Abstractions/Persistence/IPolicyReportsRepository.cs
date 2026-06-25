using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IPolicyReportsRepository
    {
        Task<List<PolicyReportRowDto>> GetPoliciesGroupedAsync(ReportDimension dimension, 
            DateTime from, 
            DateTime to,
            PolicyStatus status, 
            string currencyCode, 
            BuildingType? buildingType, 
            CancellationToken ct);
    }
}