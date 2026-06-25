using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.CancelPolicy
{
    public sealed class CancelPolicyCommandValidator : AbstractValidator<CancelPolicyCommand>
    {
        public CancelPolicyCommandValidator()
        {
            RuleFor(x => x.PolicyId)
                .NotEmpty()
                .WithMessage("PolicyId is required.");

            RuleFor(x => x.Reason)
                .Must(x => !string.IsNullOrWhiteSpace(x));

            RuleFor(x => x.CancellationEffectiveDate)
                .NotEmpty()
                .WithMessage("CancellationEffectiveDate is required.");
        }
    }
}