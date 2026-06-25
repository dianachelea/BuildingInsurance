using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.CreateBroker
{
    public sealed class CreateBrokerCommandValidator : AbstractValidator<CreateBrokerCommand>
    {
        public CreateBrokerCommandValidator()
        {
            RuleFor(x => x.BrokerCode)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Broker code is required.")
                .MinimumLength(2)
                .MaximumLength(30)
                .WithMessage("Broker code must be between 2 and 30 characters.");

            RuleFor(x => x.FullName)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Broker name is required.")
                .MinimumLength(3)
                .MaximumLength(200)
                .WithMessage("Broker name must be between 3 and 200 characters.");

            RuleFor(x => x.Email)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email is invalid.");

            RuleFor(x => x.Phone)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Phone is required.")
                .Matches(@"^\+?\d+$")
                .WithMessage("Phone must contain only digits and optional leading '+'.")
                .MaximumLength(30)
                .WithMessage("Phone must not exceed 30 characters.");

            RuleFor(x => x.CommissionPercentage)
                .GreaterThan(0m)
                .LessThan(1m)
                .WithMessage("CommissionPercentage must be between 0 (exclusive) and 1 (exclusive).")
                .When(x => x.CommissionPercentage.HasValue);
        }
    }
}