using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Queries.GetBuildingById
{
    public sealed class GetBuildingByIdHandler : IRequestHandler<GetBuildingByIdQuery, Result<BuildingDetailsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeographyCachingService _geography;

        public GetBuildingByIdHandler(IUnitOfWork unitOfWork, IGeographyCachingService geography)
        {
            _unitOfWork = unitOfWork;
            _geography = geography;
        }

        public async Task<Result<BuildingDetailsDto>> Handle(GetBuildingByIdQuery request, CancellationToken cancellationToken)
        {
            var building = await _unitOfWork.Buildings.GetByIdAsync(request.BuildingId, cancellationToken);
            if (building is null)
            {
                return Result<BuildingDetailsDto>.Failure($"Building with ID {request.BuildingId} not found.", ErrorType.NotFound);
            }

            if (!_geography.TryGet(building.CityId, out var city, out var county, out var country))
            {
                return Result<BuildingDetailsDto>.Failure("Geography not found for building.", ErrorType.NotFound);
            }

            var dto = new BuildingDetailsDto
            {
                Id = building.Id,
                ClientId = building.ClientId,
                Street = building.Address.Street,
                Number = building.Address.Number,
                City = city,
                County = county,
                Country = country,
                ConstructionYear = building.ConstructionYear,
                Type = building.Type,
                NumberOfFloors = building.NumberOfFloors,
                SurfaceArea = building.SurfaceArea,
                InsuredValue = building.InsuredValue
            };

            return Result<BuildingDetailsDto>.Success(dto);
        }
    }
}