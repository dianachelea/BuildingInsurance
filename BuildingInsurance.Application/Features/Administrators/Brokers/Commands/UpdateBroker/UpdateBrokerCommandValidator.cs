using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.UpdateBroker
{
    public sealed class UpdateBrokerCommandValidator : AbstractValidator<UpdateBrokerCommand>
    {
        public UpdateBrokerCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Broker id is required.");

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
                .Matches(@"^\d+$")
                .WithMessage("Phone must contain only numeric characters.")
                .MaximumLength(20)
                .WithMessage("Phone must not exceed 20 characters.");

            RuleFor(x => x.CommissionPercentage)
                .GreaterThan(0m)
                .LessThan(1m)
                .WithMessage("CommissionPercentage must be between 0 (exclusive) and 1 (exclusive).")
                .When(x => x.CommissionPercentage.HasValue);
        }
    }
}