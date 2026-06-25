using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries
{
    public sealed class ListCountriesQueryValidator : PaginatedQueryValidator<ListCountriesQuery>
    {
        public ListCountriesQueryValidator()
        {
        }
    }
}