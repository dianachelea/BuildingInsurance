using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy
{
    public sealed class CreateDraftPolicyCommand : ICommand<Result<PolicyDto>>
    {
        public Guid ClientId { get; set; }
        public Guid BuildingId { get; set; }
        public Guid CurrencyId { get; set; }
        public Guid BrokerId { get; set; }
        public decimal BasePremium { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}