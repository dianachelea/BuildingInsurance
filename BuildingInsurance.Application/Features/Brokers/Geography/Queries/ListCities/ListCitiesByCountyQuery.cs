using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities
{
    public sealed record ListCitiesByCountyQuery : PaginatedQuery, IRequest<Result<ListCitiesByCountyResponse>>
    {
        public Guid CountyId {  get; init; }
    }
}