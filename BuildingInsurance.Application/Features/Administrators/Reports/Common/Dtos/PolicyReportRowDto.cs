namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos
{
    public sealed record PolicyReportRowDto(string GroupingKey, 
        string CurrencyCode, 
        int PolicyCount, 
        decimal TotalFinalPremium, 
        decimal TotalFinalPremiumInBaseCurrency);
}