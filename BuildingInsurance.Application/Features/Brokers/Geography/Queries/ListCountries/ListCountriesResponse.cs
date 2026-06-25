using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries
{
    public sealed record ListCountriesResponse : PaginatedResult<CountryDto>
    {
        public ListCountriesResponse(IReadOnlyList<CountryDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}