using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.CancelPolicy
{
    public sealed class CancelPolicyCommand : ICommand<Result<PolicyDto>>
    {
        public Guid PolicyId { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime CancellationEffectiveDate { get; set; }
    }
}