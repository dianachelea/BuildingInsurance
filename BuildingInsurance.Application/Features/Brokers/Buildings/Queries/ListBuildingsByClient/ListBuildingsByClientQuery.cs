using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient
{
    public sealed record ListBuildingsByClientQuery : PaginatedQuery, IRequest<Result<ListBuildingsByClientResponse>>
    {
        public Guid ClientId { get; init; }
    }
}