using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using BuildingInsurance.Application.Features.Common.Mapping;

namespace BuildingInsurance.Application.Features.Brokers.Buildings.Commands.CreateBuilding
{
    public sealed class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, Result<BuildingDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateBuildingCommandHandler> _logger;

        public CreateBuildingCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateBuildingCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<BuildingDto>> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
            if(client is null)
            {
                _logger.LogWarning("Client with ID {ClientId} not found.", request.ClientId);
                return Result<BuildingDto>.Failure($"Client with ID {request.ClientId} not found.", ErrorType.NotFound);
            }

            var city = await _unitOfWork.Cities.GetByIdAsync(request.CityId, cancellationToken);
            if(city is null)
            {
                _logger.LogWarning("City with ID {CityId} not found.", request.CityId);
                return Result<BuildingDto>.Failure($"City with ID {request.CityId} not found.", ErrorType.NotFound);
            }

            var existinsForClientAtAddress = await _unitOfWork.Buildings
                .ExistsForClientAtAddressAsync(
                    request.ClientId,
                    request.CityId,
                    request.Street,
                    request.Number,
                    cancellationToken
                );
            if (existinsForClientAtAddress)
            {
                _logger.LogWarning("Building already exists for Client ID {ClientId} at {Street} {Number}, {CityId}.", request.ClientId, request.Street, request.Number, request.CityId);
                return Result<BuildingDto>.Conflict("Building already exists at the specified address for this client.");
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                var address = new Address(request.Street, request.Number);

                var building = new Building(
                    clientId: request.ClientId,
                    address: address,
                    cityId: request.CityId,
                    constructionYear: request.ConstructionYear,
                    type: request.Type.MapToDomainBuildingType(),
                    numberOfFloors: request.NumberOfFloors,
                    surfaceArea: request.SurfaceArea,
                    insuredValue: request.InsuredValue,
                    riskIndicators: request.RiskIndicators.MapToDomainRiskIndicators()
                    );

                await _unitOfWork.Buildings.AddAsync(building, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new BuildingDto
                {
                    Id = building.Id,
                    ClientId = building.ClientId,
                    Street = building.Address.Street,
                    Number = building.Address.Number,
                    CityId = building.CityId,
                    ConstructionYear = building.ConstructionYear,
                    Type = building.Type,
                    NumberOfFloors = building.NumberOfFloors,
                    SurfaceArea = building.SurfaceArea,
                    InsuredValue = building.InsuredValue,
                    RiskIndicators = building.RiskIndicators
                };

                return Result<BuildingDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during buildind creation for ClientId = {ClientId}", request.ClientId);
                return Result<BuildingDto>.Failure(error: "Unexpected error during buildind creation.", ErrorType.Generic);
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
                        _logger.LogWarning(finalRbEx, "Final rollback attempt failed during buildind creation for ClientId = {ClientId}", request.ClientId);
                    }
                }
            }
        }
    }
}