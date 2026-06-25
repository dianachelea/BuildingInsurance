using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient
{
    public class ListBuildingsByClientHandler : IRequestHandler<ListBuildingsByClientQuery, Result<ListBuildingsByClientResponse>>
    {
        private readonly IBuildingRepository _buildingRepository;

        public ListBuildingsByClientHandler(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public async Task<Result<ListBuildingsByClientResponse>> Handle(ListBuildingsByClientQuery request, CancellationToken cancellationToken)
        {
            var (buildings, totalCount) = await _buildingRepository.GetByClientIdPagedAsync(request.ClientId, request.Page, request.PageSize, cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = buildings.Select(b => new BuildingDto
            {
                Id = b.Id,
                ClientId = b.ClientId,
                CityId = b.CityId,
                Street = b.Address.Street,
                Number = b.Address.Number,
                ConstructionYear = b.ConstructionYear,
                Type = b.Type,
                NumberOfFloors = b.NumberOfFloors,
                SurfaceArea = b.SurfaceArea,
                InsuredValue = b.InsuredValue,
                RiskIndicators = b.RiskIndicators
            }).ToList();

            var response = new ListBuildingsByClientResponse(items, totalPages, totalCount);
            return Result<ListBuildingsByClientResponse>.Success(response);
        }
    }
}