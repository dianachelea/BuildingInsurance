using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Models
{
    public sealed record PolicyReportFilters(DateTime From, 
        DateTime To, 
        PolicyStatusContract? Status, 
        string? CurrencyCode, 
        BuildingTypeContract? BuildingType);
}