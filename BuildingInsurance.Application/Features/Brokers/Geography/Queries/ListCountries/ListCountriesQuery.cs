using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries
{
    public sealed record ListCountriesQuery : PaginatedQuery, IRequest<Result<ListCountriesResponse>>;
}