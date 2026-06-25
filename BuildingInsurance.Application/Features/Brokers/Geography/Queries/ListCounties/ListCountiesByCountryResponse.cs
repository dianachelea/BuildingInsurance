using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties
{
    public sealed record ListCountiesByCountryResponse : PaginatedResult<CountyDto>
    {
        public ListCountiesByCountryResponse(IReadOnlyList<CountyDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}