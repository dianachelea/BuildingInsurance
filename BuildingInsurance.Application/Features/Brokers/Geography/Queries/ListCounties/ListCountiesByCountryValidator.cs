using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties
{
    public sealed class ListCountiesByCountryQueryValidator : PaginatedQueryValidator<ListCountiesByCountryQuery>
    {
        public ListCountiesByCountryQueryValidator()
        {
            RuleFor(x => x.CountryId)
                .NotEmpty();
        }
    }
}