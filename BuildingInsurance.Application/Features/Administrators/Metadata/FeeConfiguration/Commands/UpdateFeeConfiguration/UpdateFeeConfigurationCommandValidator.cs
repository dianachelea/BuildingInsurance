using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.UpdateFeeConfiguration
{
    public sealed class UpdateFeeConfigurationCommandValidator : AbstractValidator<UpdateFeeConfigurationCommand>
    {
        public UpdateFeeConfigurationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Fee configuration id is required.");

            RuleFor(x => x.Name)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Fee name is required.")
                .MinimumLength(3)
                .MaximumLength(100)
                .WithMessage("Fee name must be between 3 and 100 characters.");

            RuleFor(x => x.FeePercentage)
                .GreaterThan(0m)
                .LessThan(1m)
                .WithMessage("Fee percentage must be between 0 and 1.");

            RuleFor(x => x.EffectiveTo)
                .GreaterThan(x => x.EffectiveFrom)
                .WithMessage("EffectiveTo must be after EffectiveFrom.");

            RuleFor(x => x.FeeType)
                .IsInEnum()
                .WithMessage("Fee type is invalid.");

            When(x => x.FeeType == FeeTypeContract.RiskAdjustment, () =>
            {
                RuleFor(x => x.RiskIndicators)
                    .NotEqual(RiskIndicatorsContract.None)
                    .WithMessage("RiskAdjustment fee must specify risk indicators.");
            });

            When(x => x.FeeType != FeeTypeContract.RiskAdjustment, () =>
            {
                RuleFor(x => x.RiskIndicators)
                    .Equal(RiskIndicatorsContract.None)
                    .WithMessage("Only RiskAdjustment fees can have risk indicators.");
            });
        }
    }
}