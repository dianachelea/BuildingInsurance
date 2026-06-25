using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.CreateCurrency
{
    public sealed class CreateCurrencyCommand : ICommand<Result<CurrencyDto>>
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; }
    }
}