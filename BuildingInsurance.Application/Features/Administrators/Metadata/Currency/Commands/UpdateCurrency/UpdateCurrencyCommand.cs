using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.UpdateCurrency
{
    public sealed class UpdateCurrencyCommand : ICommand<Result<CurrencyDto>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; }
    }
}