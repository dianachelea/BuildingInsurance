using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Queries.GetBrokerById
{
    public sealed record GetBrokerByIdQuery(Guid BrokerId) : IRequest<Result<BrokerDto>>;
}