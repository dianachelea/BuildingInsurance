using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient
{
    public class CreateClientCommand : ICommand<Result<ClientDto>>
    {
        public ClientTypeContract Type { get; set; }
        public string FullName { get; set; } = null!;
        public string? PersonalIdentificationNumber { get; set; }
        public string? CompanyRegistrationNumber { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public AddressDto? Address { get; set; }
    }
}