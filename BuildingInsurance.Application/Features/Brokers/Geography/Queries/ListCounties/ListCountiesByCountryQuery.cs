using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties
{
    public sealed record ListCountiesByCountryQuery : PaginatedQuery, IRequest<Result<ListCountiesByCountryResponse>>
    {
        public Guid CountryId { get; init; }
    }
}