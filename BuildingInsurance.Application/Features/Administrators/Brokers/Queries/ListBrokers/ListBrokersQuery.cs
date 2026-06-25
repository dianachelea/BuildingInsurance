using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers
{
    public sealed record ListBrokersQuery : PaginatedQuery, IRequest<Result<ListBrokersResponse>>
    {
        public string? Name { get; init; }
        public bool? IsActive { get; init; }
    }
}