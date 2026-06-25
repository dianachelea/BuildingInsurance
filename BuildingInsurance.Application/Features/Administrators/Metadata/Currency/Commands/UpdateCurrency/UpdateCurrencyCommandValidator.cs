using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.UpdateCurrency
{
    public sealed class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
    {
        public UpdateCurrencyCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Currency id is required.");

            RuleFor(x => x.Name)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Currency name is required.")
                .MaximumLength(100)
                .WithMessage("Currency name must not exceed 100 characters.");

            RuleFor(x => x.ExchangeRateToBase)
                .GreaterThan(0)
                .WithMessage("ExchangeRateToBase must be greater than 0.");
        }
    }
}