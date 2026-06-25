using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Commands.UpdateClient
{
    public class UpdateClientCommand : ICommand<Result<ClientDto>>
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public AddressDto? Address { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationChangeReason { get; set; }
    }
}