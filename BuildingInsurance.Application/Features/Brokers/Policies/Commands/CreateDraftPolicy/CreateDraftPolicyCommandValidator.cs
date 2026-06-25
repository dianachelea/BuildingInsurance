using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy
{
    public sealed class CreateDraftPolicyCommandValidator : AbstractValidator<CreateDraftPolicyCommand>
    {
        public CreateDraftPolicyCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("ClientId is required.");

            RuleFor(x => x.BuildingId)
                .NotEmpty()
                .WithMessage("BuildingId is required.");

            RuleFor(x => x.CurrencyId)
                .NotEmpty()
                .WithMessage("CurrencyId is required.");

            RuleFor(x => x.BrokerId)
                .NotEmpty()
                .WithMessage("BrokerId is required.");

            RuleFor(x => x.BasePremium)
                .GreaterThan(0m)
                .WithMessage("BasePremium must be greater than 0.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate.");
        }
    }
}