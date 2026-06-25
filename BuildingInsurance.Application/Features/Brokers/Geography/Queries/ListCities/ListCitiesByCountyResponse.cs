using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities
{
    public sealed record ListCitiesByCountyResponse : PaginatedResult<CityDto>
    {
        public ListCitiesByCountyResponse(IReadOnlyList<CityDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}