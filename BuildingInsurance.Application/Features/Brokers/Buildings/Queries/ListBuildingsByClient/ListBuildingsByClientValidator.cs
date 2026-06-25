using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient
{
    public sealed class ListBuildingsByClientValidator : PaginatedQueryValidator<ListBuildingsByClientQuery>
    {
        public ListBuildingsByClientValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("ClientId is required.");
        }
    }
}