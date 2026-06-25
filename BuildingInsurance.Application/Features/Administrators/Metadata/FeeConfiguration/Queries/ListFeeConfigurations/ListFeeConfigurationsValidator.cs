using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations
{
    public sealed class ListFeeConfigurationsValidator : PaginatedQueryValidator<ListFeeConfigurationsQuery>
    {
        public ListFeeConfigurationsValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Fee name must not exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Fee type is invalid.")
                .When(x => x.Type.HasValue);
        }
    }
}