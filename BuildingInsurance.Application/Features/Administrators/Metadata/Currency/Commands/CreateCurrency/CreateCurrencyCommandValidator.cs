using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.CreateCurrency
{
    public sealed class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
    {
        public CreateCurrencyCommandValidator()
        {
            RuleFor(x => x.Code)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .WithMessage("Currency code is required.")
                    .Must(code =>
                    {
                        var c = code!.Trim();
                        return c.Length == 3 && c.All(ch => ch is >= 'A' and <= 'Z');
                    })
                    .WithMessage("Currency code is invalid.");

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