using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities
{
    public sealed class ListCitiesByCountyQueryValidator : PaginatedQueryValidator<ListCitiesByCountyQuery>
    {
        public ListCitiesByCountyQueryValidator()
        {
            RuleFor(x => x.CountyId)
                .NotEmpty();
        }
    }
}