namespace BuildingInsurance.API.Contracts.Brokers.Clients
{
    public class UpdateClientRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public AddressRequestDto? Address { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationChangeReason { get; set; }
    }
}