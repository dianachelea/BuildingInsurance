using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient
{
    public sealed record ListBuildingsByClientResponse : PaginatedResult<BuildingDto>
    {
        public ListBuildingsByClientResponse(IReadOnlyList<BuildingDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}