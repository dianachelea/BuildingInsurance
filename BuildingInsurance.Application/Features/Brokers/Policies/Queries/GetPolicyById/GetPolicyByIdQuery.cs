using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Queries.GetPolicyById
{
    public sealed record GetPolicyByIdQuery(Guid PolicyId) : IRequest<Result<PolicyDetailsDto>>;
}