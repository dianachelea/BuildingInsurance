using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies
{
    public sealed record ListPoliciesQuery : PaginatedQuery, IRequest<Result<ListPoliciesResponse>>
    {
        public Guid? ClientId { get; init; }
        public Guid? BrokerId { get; init; }
        public PolicyStatusContract? Status { get; init; }
    }
}