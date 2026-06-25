using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using BuildingInsurance.Application.Features.Common.Mapping;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Commands.UpdateBuilding
{
    public sealed class UpdateBuildingCommandHandler : IRequestHandler<UpdateBuildingCommand, Result<BuildingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateBuildingCommandHandler> _logger;

        public UpdateBuildingCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateBuildingCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<BuildingDto>> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
        {
            var building = await _unitOfWork.Buildings.GetByIdAsync(request.BuildingId, cancellationToken);
            if (building is null)
            {
                _logger.LogWarning("Building with ID {BuildingId} not found.", request.BuildingId);
                return Result<BuildingDto>.Failure($"Building with ID {request.BuildingId} not found.", ErrorType.NotFound);
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                var newAddress = new Address(request.Address.Street, request.Address.Number);

                building.Relocate(newAddress, request.CityId);

                building.UpdateConstruction(request.ConstructionYear, request.Type.MapToDomainBuildingType(), request.NumberOfFloors, request.SurfaceArea);

                building.UpdateInsuredValue(request.InsuredValue);

                building.UpdateRiskIndicators(request.RiskIndicators.MapToDomainRiskIndicators());

                _unitOfWork.Buildings.Update(building);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                _logger.LogInformation("Building with ID {BuildingId} updated successfully.", request.BuildingId);

                var dto = new BuildingDto
                {
                    Id = building.Id,
                    ClientId = building.ClientId,
                    CityId = building.CityId,
                    ConstructionYear = building.ConstructionYear,
                    Type = building.Type,
                    NumberOfFloors = building.NumberOfFloors,
                    SurfaceArea = building.SurfaceArea,
                    InsuredValue = building.InsuredValue,
                    RiskIndicators = building.RiskIndicators,
                    Street = building.Address.Street,
                    Number = building.Address.Number
                };

                return Result<BuildingDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating building with ID {BuildingId}.", request.BuildingId);
                return Result<BuildingDto>.Failure("An error occurred while updating the building.", ErrorType.Generic);
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                    }
                    catch (Exception finalRbEx)
                    {
                        _logger.LogWarning(finalRbEx, "Final rollback attempt failed while updating the building with BuildingId = {BuildingId}", request.BuildingId);
                    }
                }
            }
        }
    }
}