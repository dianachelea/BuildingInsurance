using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
    {
        public UpdateClientCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required.");

            RuleFor(x => x.FullName)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Full name is required.")
                .MaximumLength(200)
                .WithMessage("Full name must not exceed 200 characters.");

            RuleFor(x => x.Email)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email format is invalid.")
                .MaximumLength(200)
                .WithMessage("Email must not exceed 200 characters.");

            RuleFor(x => x.Phone)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Phone number is required.")
                .MaximumLength(20)
                .Matches(@"^\+?\d+$")
                .WithMessage("Phone must contain only digits and optional leading '+'.")
                .WithMessage("Phone number must not exceed 20 characters.");

            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address!.Street)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Street is required.")
                .MaximumLength(200)
                .WithMessage("Street must not exceed 200 characters.");

                RuleFor(x => x.Address!.Number)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Address number is required.")
                .MaximumLength(20)
                .WithMessage("Address number must not exceed 20 characters.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.IdentificationNumber), () =>
            {
                RuleFor(x => x.IdentificationChangeReason)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Reason is required when changing identification number.");

                RuleFor(x => x.IdentificationNumber!)
                .MaximumLength(20)
                .WithMessage("Identification number must not exceed 20 characters.");
            });
        }
    }
}