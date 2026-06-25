using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.GetCurrencyById
{
    public sealed record GetCurrencyByIdQuery(Guid CurrencyId) : IRequest<Result<CurrencyDto>>;
}