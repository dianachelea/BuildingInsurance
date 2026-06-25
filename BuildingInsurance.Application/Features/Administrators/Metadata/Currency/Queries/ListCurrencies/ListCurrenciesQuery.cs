using BuildingInsurance.Application.Features.Common.Requests;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies
{
    public sealed record ListCurrenciesQuery: PaginatedQuery, IRequest<Result<ListCurrenciesResponse>>
    {
        public string? Name { get; init; }
        public bool? IsActive { get; init; }
    }
}