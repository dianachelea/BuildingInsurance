using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.ActivateBroker
{
    public sealed class ActivateBrokerCommandValidator : AbstractValidator<ActivateBrokerCommand>
    {
        public ActivateBrokerCommandValidator()
        {
            RuleFor(x => x.BrokerId)
                .NotEmpty()
                .WithMessage("BrokerId is required.");
        }
    }
}