using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients
{
    public sealed record ListClientsQuery: PaginatedQuery, IRequest<Result<ListClientsResponse>>
    {
        public string? Name { get; init; }
        public string? Identifier { get; init; }
    }
}