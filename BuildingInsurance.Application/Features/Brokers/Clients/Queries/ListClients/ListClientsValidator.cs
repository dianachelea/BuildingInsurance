using BuildingInsurance.Application.Features.Common.Validation;
using FluentValidation;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients
{
    public sealed class ListClientsValidator : PaginatedQueryValidator<ListClientsQuery>
    {
        public ListClientsValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Identifier)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.Identifier));
        }
    }
}