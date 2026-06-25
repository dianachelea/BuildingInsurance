using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations
{
    public sealed class ListRiskFactorConfigurationsValidator : PaginatedQueryValidator<ListRiskFactorConfigurationsQuery>
    {
        public ListRiskFactorConfigurationsValidator()
        {
            RuleFor(x => x.Level)
                .IsInEnum()
                .WithMessage("Risk factor level is invalid.")
                .When(x => x.Level.HasValue);

            RuleFor(x => x.ReferenceId)
                .Must(id => id is null || id.Value != Guid.Empty)
                .WithMessage("ReferenceId must be a valid GUID.")
                .When(x => x.ReferenceId.HasValue);
        }
    }
}