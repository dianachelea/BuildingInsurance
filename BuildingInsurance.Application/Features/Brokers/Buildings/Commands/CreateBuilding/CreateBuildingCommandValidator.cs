using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Commands.CreateBuilding
{
    public sealed class CreateBuildingCommandValidator : AbstractValidator<CreateBuildingCommand>
    {
        public CreateBuildingCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required.");

            RuleFor(x => x.CityId)
                .NotEmpty()
                .WithMessage("CityId is required.");

            RuleFor(x => x.Street)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Address street is required.")
                .MaximumLength(200)
                .WithMessage("Address street must not exceed 200 characters.");

            RuleFor(x => x.Number)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Address number is required.")
                .MaximumLength(20)
                .WithMessage("Address number must not exceed 20 characters.");

            RuleFor(x => x.ConstructionYear)
                .InclusiveBetween(1800, DateTime.UtcNow.Year)
                .WithMessage($"Construction year must be between 1800 and {DateTime.UtcNow.Year}.");

            RuleFor(x => x.NumberOfFloors)
                .GreaterThan(0)
                .WithMessage("Number of floors must be greater than 0.");

            RuleFor(x => x.SurfaceArea)
                .GreaterThan(0)
                .WithMessage("Surface area must be greater than 0.");

            RuleFor(x => x.InsuredValue)
                .GreaterThan(0)
                .WithMessage("Insured value must be greater than 0.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Building type is invalid.");

            RuleFor(x => x.RiskIndicators)
                .Must(r => (int)r >= 0)
                .WithMessage("Risk indicators value is invalid.");
        }
    }
}