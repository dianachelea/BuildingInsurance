using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations
{
    public sealed record ListRiskFactorConfigurationsQuery: PaginatedQuery, IRequest<Result<ListRiskFactorConfigurationsResponse>>
    {
        public RiskFactorLevelContract? Level { get; init; }
        public Guid? ReferenceId { get; init; }
        public bool? IsActive { get; init; }
    }
}