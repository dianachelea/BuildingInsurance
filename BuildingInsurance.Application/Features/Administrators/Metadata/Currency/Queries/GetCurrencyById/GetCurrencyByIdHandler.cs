using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.GetCurrencyById
{
    public sealed class GetCurrencyByIdHandler : IRequestHandler<GetCurrencyByIdQuery, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCurrencyByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CurrencyDto>> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(request.CurrencyId, cancellationToken);
            if(currency is null)
            {
                return Result<CurrencyDto>.Failure("Currency not found.", ErrorType.NotFound);
            }

            var currencyDto = new CurrencyDto
            {
                Id = currency.Id,
                Code = currency.Code,
                Name = currency.Name,
                ExchangeRateToBase = currency.ExchangeRateToBase,
                IsActive = currency.IsActive
            };

            return Result<CurrencyDto>.Success(currencyDto);
        }
    }
}