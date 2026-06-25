using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies
{
    public sealed class ListCurrenciesValidator : PaginatedQueryValidator<ListCurrenciesQuery>
    {
        public ListCurrenciesValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Currency name must not exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));
        }
    }
}