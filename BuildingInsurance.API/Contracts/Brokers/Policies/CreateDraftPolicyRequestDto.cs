namespace BuildingInsurance.API.Contracts.Brokers.Policies
{
    public sealed class CreateDraftPolicyRequestDto
    {
        public Guid ClientId { get; set; }
        public Guid BuildingId { get; set; }
        public Guid CurrencyId { get; set; }
        public Guid BrokerId { get; set; }
        public decimal BasePremium { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}