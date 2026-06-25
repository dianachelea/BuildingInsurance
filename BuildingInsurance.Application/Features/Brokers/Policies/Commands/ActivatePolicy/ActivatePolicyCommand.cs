using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy
{
    public sealed record ActivatePolicyCommand(Guid PolicyId) : ICommand<Result<PolicyDto>>;
}