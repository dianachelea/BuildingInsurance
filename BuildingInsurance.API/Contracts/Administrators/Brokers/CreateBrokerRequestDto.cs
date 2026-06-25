namespace BuildingInsurance.API.Contracts.Administrators.Brokers
{
    public sealed class CreateBrokerRequestDto
    {
        public string BrokerCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public decimal? CommissionPercentage { get; set; }
    }
}