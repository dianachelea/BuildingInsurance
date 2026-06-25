using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Commands.CreateReportJob
{
    public sealed class CreateReportJobCommandValidator : AbstractValidator<CreateReportJobCommand>
    {
        public CreateReportJobCommandValidator()
        {
            RuleFor(x => x.Dimension)
                .IsInEnum()
                .WithMessage("Report dimension is invalid.");

            RuleFor(x => x.Filters)
                .NotNull()
                .WithMessage("Filters are required.");

            When(x => x.Filters is not null, () =>
            {
                RuleFor(x => x.Filters.From)
                    .NotEmpty()
                    .WithMessage("From date is required.");

                RuleFor(x => x.Filters.To)
                    .NotEmpty()
                    .WithMessage("To date is required.");

                RuleFor(x => x)
                    .Must(x => x.Filters.From <= x.Filters.To)
                    .WithMessage("From must be earlier than or equal to To.");

                RuleFor(x => x.Filters.Status)
                    .NotNull()
                    .WithMessage("Policy status is required.");

                RuleFor(x => x.Filters.Status)
                    .Must(s =>
                        s == PolicyStatusContract.Active ||
                        s == PolicyStatusContract.Expired ||
                        s == PolicyStatusContract.Cancelled)
                    .When(x => x.Filters.Status.HasValue)
                    .WithMessage("Policy status must be Active, Expired, or Cancelled.");

                RuleFor(x => x.Filters.CurrencyCode)
                    .Must(code => !string.IsNullOrWhiteSpace(code))
                    .WithMessage("Currency code is required.")
                    .Must(code =>
                    {
                        var c = code!.Trim();
                        return c.Length == 3 && c.All(char.IsLetter);
                    })
                    .WithMessage("Currency code is invalid.");

                RuleFor(x => x.Filters.BuildingType)
                    .IsInEnum()
                    .When(x => x.Filters.BuildingType.HasValue)
                    .WithMessage("Building type is invalid.");
            });
        }
    }
}