using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies
{
    public sealed class ListPoliciesValidator : PaginatedQueryValidator<ListPoliciesQuery>
    {
        public ListPoliciesValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEqual(Guid.Empty)
                .When(x => x.ClientId.HasValue)
                .WithMessage("ClientId is invalid.");

            RuleFor(x => x.BrokerId)
                .NotEqual(Guid.Empty)
                .When(x => x.BrokerId.HasValue)
                .WithMessage("BrokerId is invalid.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("Policy status is invalid.");
        }
    }
}