namespace BuildingInsurance.API.Contracts.Administrators.Currencies
{
    public sealed class CreateCurrencyRequestDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; }
    }
}