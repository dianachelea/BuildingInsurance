using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Commands.UpdateBuilding
{
    public sealed class UpdateBuildingCommandValidator : AbstractValidator<UpdateBuildingCommand>
    {
        public UpdateBuildingCommandValidator()
        {
            RuleFor(x => x.BuildingId)
                .NotEmpty()
                .WithMessage("BuildingId is required.");

            RuleFor(x => x.CityId)
                .NotEmpty()
                .WithMessage("CityId is required.");

            RuleFor(x => x.Address)
                .NotNull()
                .WithMessage("Address is required.");

            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address.Street)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Address street is required.")
                .MaximumLength(200)
                .WithMessage("Address street must not exceed 200 characters.");

                RuleFor(x => x.Address.Number)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Address number is required.")
                .MaximumLength(20)
                .WithMessage("Address number must not exceed 20 characters.");
            });


            RuleFor(x => x.ConstructionYear)
                .InclusiveBetween(1800, DateTime.UtcNow.Year)
                .WithMessage($"Construction year must be between 1800 and {DateTime.UtcNow.Year}.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Building type is invalid.");

            RuleFor(x => x.NumberOfFloors)
                .GreaterThan(0)
                .WithMessage("Number of floors must be greater than 0.");

            RuleFor(x => x.SurfaceArea)
                .GreaterThan(0)
                .WithMessage("Surface area must be greater than 0.");

            RuleFor(x => x.InsuredValue)
                .GreaterThan(0)
                .WithMessage("Insured value must be greater than 0.");

            RuleFor(x => x.RiskIndicators)
                .Must(x => (int)x >= 0)
                .WithMessage("Risk indicators value is invalid.");
        }
    }
}