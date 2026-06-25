namespace BuildingInsurance.API.Contracts.Brokers.Clients
{
    public class AddressRequestDto
    {
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
    }
}