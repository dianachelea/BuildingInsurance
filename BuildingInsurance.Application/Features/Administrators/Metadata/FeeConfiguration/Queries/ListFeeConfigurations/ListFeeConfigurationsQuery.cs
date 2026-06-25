using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations
{
    public sealed record ListFeeConfigurationsQuery : PaginatedQuery, IRequest<Result<ListFeeConfigurationsResponse>>
    {
        public string? Name { get; init; }
        public FeeTypeContract? Type { get; init; }
        public bool? IsActive { get; init; }
    }
}