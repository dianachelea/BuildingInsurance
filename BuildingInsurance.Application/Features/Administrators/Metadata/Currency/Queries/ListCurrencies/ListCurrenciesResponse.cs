using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies
{
    public sealed record ListCurrenciesResponse : PaginatedResult<CurrencyDto>
    {
        public ListCurrenciesResponse(IReadOnlyList<CurrencyDto> Items, int TotalPages, int TotalCount) : base(Items, TotalPages, TotalCount)
        {
        }
    }
}