using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class ClientDto
    {
        public Guid Id { get; set; }
        public ClientType Type { get; set; }
        public string FullName { get; set; } = null!;
        public string? PersonalIdentificationNumber { get; set; }
        public string? CompanyRegistrationNumber { get; set; }
        public string Email { get; set; } = null!; 
        public string Phone { get; set; } = null!;

        public static implicit operator ClientDto(Client client) 
        { 
            return new ClientDto { 
                Id = client.Id, 
                Type = client.Type, 
                FullName = client.FullName, 
                PersonalIdentificationNumber = client.PersonalIdentificationNumber, 
                CompanyRegistrationNumber = client.CompanyRegistrationNumber, 
                Email = client.ContactInfo.Email, 
                Phone = client.ContactInfo.Phone 
            }; 
        }
    }
}