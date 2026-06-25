using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies
{
    public sealed class ListCurrenciesHandler : IRequestHandler<ListCurrenciesQuery, Result<ListCurrenciesResponse>>
    {
        private readonly ICurrencyRepository _currencyRepository;

        public ListCurrenciesHandler(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<Result<ListCurrenciesResponse>> Handle(ListCurrenciesQuery request, CancellationToken cancellationToken)
        {
            var (currencies, totalCount) = await _currencyRepository.SearchPagedAsync(request.Name, request.IsActive, request.Page, request.PageSize, cancellationToken);
            
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = currencies.Select(c => new CurrencyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ExchangeRateToBase = c.ExchangeRateToBase,
                IsActive = c.IsActive
            }).ToList();

            var response = new ListCurrenciesResponse(items, totalPages, totalCount);
            return Result<ListCurrenciesResponse>.Success(response);
        }
    }
}