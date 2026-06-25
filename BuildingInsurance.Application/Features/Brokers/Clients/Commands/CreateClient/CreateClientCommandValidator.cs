using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient
{
    public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
    {
        public CreateClientCommandValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Client type is invalid.");

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

            When(x => x.Type == ClientTypeContract.Individual, () =>
            {
                RuleFor(x => x.PersonalIdentificationNumber)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .WithMessage("Personal identification number is required for individual clients.");

                RuleFor(x => x.CompanyRegistrationNumber)
                    .Must(string.IsNullOrWhiteSpace)
                    .WithMessage("Company registration number must be empty for individual clients.");
            });

            When(x => x.Type == ClientTypeContract.Company, () =>
            {
                RuleFor(x => x.CompanyRegistrationNumber)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .WithMessage("Company registration number is required for company clients.");

                RuleFor(x => x.PersonalIdentificationNumber)
                    .Must(string.IsNullOrWhiteSpace)
                    .WithMessage("Personal identification number must be empty for company clients.");
            });

            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address!.Street)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Address street is required.")
                .MaximumLength(200)
                .WithMessage("Address street must not exceed 200 characters.");

                RuleFor(x => x.Address!.Number)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Address number is required.")
                .MaximumLength(20)
                .WithMessage("Address number must not exceed 20 characters.");
            });
        }
    }
}