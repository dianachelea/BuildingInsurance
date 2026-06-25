using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Queries.GetClient
{
    public sealed record GetClientByIdQuery(Guid ClientId) : IRequest<Result<ClientDetailsDto>>;
}