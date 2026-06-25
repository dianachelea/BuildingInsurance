using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy
{
    public sealed class ActivatePolicyCommandValidator : AbstractValidator<ActivatePolicyCommand>
    {
        public ActivatePolicyCommandValidator()
        {
            RuleFor(x => x.PolicyId)
                .NotEmpty()
                .WithMessage("PolicyId is required.");
        }
    }
}