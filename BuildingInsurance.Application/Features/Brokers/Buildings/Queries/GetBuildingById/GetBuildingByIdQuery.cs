using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Queries.GetBuildingById
{
    public sealed record GetBuildingByIdQuery(Guid BuildingId) : IRequest<Result<BuildingDetailsDto>>;
}