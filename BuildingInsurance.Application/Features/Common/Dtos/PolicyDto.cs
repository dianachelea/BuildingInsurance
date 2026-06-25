using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class PolicyDto
    {
        public Guid Id { get; set; }
        public string PolicyNumber { get; set; } = null!;
        public Guid ClientId { get; set; }
        public Guid BuildingId { get; set; }
        public Guid BrokerId { get; set; }
        public Guid CurrencyId { get; set; }
        public PolicyStatus PolicyStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal BasePremium { get; set; }
        public decimal FinalPremium { get; set; }
        public decimal? EstimatedFinalPremium { get; set; }
        public decimal FinalPremiumInBaseCurrency { get; set; }
        public DateTime? CancellationEffectiveDate { get; set; }
    }
}