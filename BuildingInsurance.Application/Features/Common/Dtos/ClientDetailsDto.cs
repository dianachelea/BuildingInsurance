using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class ClientDetailsDto
    {
        public Guid Id { get; init; }
        public ClientType Type { get; init; }
        public string FullName { get; init; } = null!;
        public string? PersonalIdentificationNumber { get; init; }
        public string? CompanyRegistrationNumber { get; init; }
        public string Email { get; init; } = null!;
        public string Phone { get; init; } = null!;
        public AddressDto? Address { get; init; } = null!;
    }
}