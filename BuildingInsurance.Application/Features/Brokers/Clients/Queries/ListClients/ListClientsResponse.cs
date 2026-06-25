using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients
{
    public sealed record ListClientsResponse : PaginatedResult<ClientDto>
    {
        public ListClientsResponse(IReadOnlyList<ClientDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}