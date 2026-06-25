namespace BuildingInsurance.API.Contracts.Brokers.Clients
{
    public class CreateClientRequestDto
    {
        public ClientTypeRequestDto Type { get; set; }
        public string FullName { get; set; } = string.Empty; 
        public string? PersonalIdentificationNumber { get; set; } 
        public string? CompanyRegistrationNumber { get; set; }
        public string Email { get; set; } = string.Empty; 
        public string Phone { get; set; } = string.Empty; 
        public AddressRequestDto? Address { get; set; }
    }
}