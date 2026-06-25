using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations
{
    public sealed record ListFeeConfigurationsResponse : PaginatedResult<FeeConfigurationDto>
    {
        public ListFeeConfigurationsResponse(IReadOnlyList<FeeConfigurationDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}