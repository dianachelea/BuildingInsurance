namespace BuildingInsurance.API.Contracts.Administrators.Currencies
{
    public sealed class UpdateCurrencyRequestDto
    {
        public string Name { get; set; } = null!;
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; }
    }
}