using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers
{
    public sealed class ListBrokersValidator : PaginatedQueryValidator<ListBrokersQuery>
    {
        public ListBrokersValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                .WithMessage("Name must not exceed 200 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));
        }
    }
}