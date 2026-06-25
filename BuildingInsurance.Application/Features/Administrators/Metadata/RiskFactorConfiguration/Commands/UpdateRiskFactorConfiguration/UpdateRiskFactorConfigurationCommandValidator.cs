using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.UpdateRiskFactorConfiguration
{
    public sealed class UpdateRiskFactorConfigurationCommandValidator : AbstractValidator<UpdateRiskFactorConfigurationCommand>
    {
        public UpdateRiskFactorConfigurationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");

            RuleFor(x => x.Level)
                .IsInEnum()
                .WithMessage("Risk factor level is invalid.");

            RuleFor(x => x.AdjustmentPercentage)
                .GreaterThan(-1m)
                .LessThan(1m)
                .NotEqual(0m)
                .WithMessage("RiskFactorConfiguration percentage must be between -1 and 1 (exclusive) and cannot be 0.");

            When(x => x.Level == RiskFactorLevelContract.BuildingType, () =>
            {
                RuleFor(x => x.BuildingType)
                    .NotNull()
                    .WithMessage("BuildingType is required when Level is BuildingType.");

                RuleFor(x => x.ReferenceId)
                    .Must(x => x is null)
                    .WithMessage("ReferenceId must be null when Level is BuildingType.");
            });

            When(x => x.Level != RiskFactorLevelContract.BuildingType, () =>
            {
                RuleFor(x => x.ReferenceId)
                    .NotNull()
                    .WithMessage("ReferenceId is required for geographic levels.")
                    .Must(x => x.HasValue && x.Value != Guid.Empty)
                    .WithMessage("ReferenceId must be a non-empty GUID.");

                RuleFor(x => x.BuildingType)
                    .Must(x => x is null)
                    .WithMessage("BuildingType must be null for geographic levels.");
            });
        }
    }
}