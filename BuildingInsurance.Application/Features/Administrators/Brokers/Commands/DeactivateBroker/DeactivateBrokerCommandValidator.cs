using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.DeactivateBroker
{
    public sealed class DeactivateBrokerCommandValidator : AbstractValidator<DeactivateBrokerCommand>
    {
        public DeactivateBrokerCommandValidator()
        {
            RuleFor(x => x.BrokerId)
                .NotEmpty()
                .WithMessage("BrokerId is required.");
        }
    }
}