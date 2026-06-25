using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies
{
    public sealed record ListPoliciesResponse : PaginatedResult<PolicyDto>
    {
        public ListPoliciesResponse(IReadOnlyList<PolicyDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}