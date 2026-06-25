using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Queries.GetClient
{
    public class GetClientByIdHandler(IClientRepository clientRepository) : IRequestHandler<GetClientByIdQuery, Result<ClientDetailsDto>>
    {
        public async Task<Result<ClientDetailsDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
        {
            var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken);

            if (client is null)
                return Result<ClientDetailsDto>.Failure($"Client with id {request.ClientId} not found.", ErrorType.NotFound);

            var dto = new ClientDetailsDto
            {
                Id = client.Id,
                Type = client.Type,
                FullName = client.FullName,
                PersonalIdentificationNumber = client.PersonalIdentificationNumber,
                CompanyRegistrationNumber = client.CompanyRegistrationNumber,
                Email = client.ContactInfo.Email,
                Phone = client.ContactInfo.Phone,
                Address = client.ContactInfo.Address is null ? null : new AddressDto
                {
                    Street = client.ContactInfo.Address.Street,
                    Number = client.ContactInfo.Address.Number,
                }
            };

            return Result<ClientDetailsDto>.Success(dto);
        }
    }
}