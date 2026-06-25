using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients
{
    public sealed class ListClientsHandler : IRequestHandler<ListClientsQuery, Result<ListClientsResponse>>
    {
        private readonly IClientRepository _clientRepository;

        public ListClientsHandler(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<Result<ListClientsResponse>> Handle(ListClientsQuery request, CancellationToken cancellationToken)
        {
            var (clients, totalCount) = await _clientRepository.SearchPagedAsync(request.Name, request.Identifier, request.Page, request.PageSize, cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = clients
                .Select(c => new ClientDto
                {
                    Id = c.Id,
                    Type = c.Type,
                    FullName = c.FullName,
                    PersonalIdentificationNumber = c.PersonalIdentificationNumber,
                    CompanyRegistrationNumber = c.CompanyRegistrationNumber,
                    Email = c.ContactInfo.Email,
                    Phone = c.ContactInfo.Phone
                })
                .ToList();

            var response = new ListClientsResponse(items, totalPages, totalCount);

            return Result<ListClientsResponse>.Success(response);
        }
    }
}