using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Infrastructure.Persistence.Reporting
{
    public sealed class PolicyReportFact
    {
        public Guid PolicyId { get; set; }
        public DateTime StartDate { get; set; }
        public PolicyStatus PolicyStatus { get; set; }
        public Guid CurrencyId { get; set; }
        public decimal FinalPremium { get; set; }
        public decimal FinalPremiumInBaseCurrency { get; set; }
        public Guid BrokerId { get; set; }
        public string BrokerCode { get; set; } = string.Empty;
        public Guid CityId { get; set; }
        public BuildingType BuildingType { get; set; }
        public DateTime SourceLastUpdatedUtc { get; set; }
    }
}