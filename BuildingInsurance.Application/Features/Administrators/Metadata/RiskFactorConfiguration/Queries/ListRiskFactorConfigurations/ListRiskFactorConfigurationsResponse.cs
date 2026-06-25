using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations
{
    public sealed record ListRiskFactorConfigurationsResponse : PaginatedResult<RiskFactorConfigurationDto>
    {
        public ListRiskFactorConfigurationsResponse(IReadOnlyList<RiskFactorConfigurationDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}