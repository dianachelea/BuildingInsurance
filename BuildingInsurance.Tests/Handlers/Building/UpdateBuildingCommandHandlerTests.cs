using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.UpdateBuilding;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Building
{
    public class UpdateBuildingCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBuildingRepository> _buildingRepositoryMock;
        private readonly Mock<ILogger<UpdateBuildingCommandHandler>> _loggerMock;
        private readonly UpdateBuildingCommandHandler _handler;

        public UpdateBuildingCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _buildingRepositoryMock = new Mock<IBuildingRepository>();
            _loggerMock = new Mock<ILogger<UpdateBuildingCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Buildings).Returns(_buildingRepositoryMock.Object);

            _handler = new UpdateBuildingCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBuildingDoesNotExist()
        {
            var command = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                Address = new AddressDto { Street = "New St", Number = "1" },
                CityId = Guid.NewGuid(),
                ConstructionYear = 2000,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 120m,
                InsuredValue = 150_000m,
                RiskIndicators = RiskIndicatorsContract.EarthquakeProne
            };

            _buildingRepositoryMock
                .Setup(r => r.GetByIdAsync(command.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Buildings.Building?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Building with ID {command.BuildingId} not found.", result.Error);

            _buildingRepositoryMock.Verify(r => r.GetByIdAsync(command.BuildingId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUpdateSucceeds()
        {
            var buildingId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var originalCityId = Guid.NewGuid();

            var address = new Address(street: "Old St", number: "10");
            var risk = new RiskIndicators();

            var building = new Domain.Entities.Buildings.Building(
                id: buildingId,
                clientId: clientId,
                address: address,
                cityId: originalCityId,
                constructionYear: 1990,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 100m,
                insuredValue: 100_000m,
                riskIndicators: risk);

            var newCityId = Guid.NewGuid();
            var command = new UpdateBuildingCommand
            {
                BuildingId = buildingId,
                Address = new AddressDto { Street = "New St", Number = "99" },
                CityId = newCityId,
                ConstructionYear = 2010,
                Type = BuildingTypeContract.Industrial,
                NumberOfFloors = 5,
                SurfaceArea = 400m,
                InsuredValue = 450_000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            _buildingRepositoryMock
                .Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var dto = result.Value!;

            Assert.Equal(buildingId, dto.Id);
            Assert.Equal(clientId, dto.ClientId);
            Assert.Equal(command.CityId, dto.CityId);
            Assert.Equal(command.ConstructionYear, dto.ConstructionYear);
            Assert.Equal(command.Type.MapToDomainBuildingType(), dto.Type);
            Assert.Equal(command.NumberOfFloors, dto.NumberOfFloors);
            Assert.Equal(command.SurfaceArea, dto.SurfaceArea);
            Assert.Equal(command.InsuredValue, dto.InsuredValue);
            Assert.Equal(command.Address.Street.ToUpperInvariant(), dto.Street);
            Assert.Equal(command.Address.Number, dto.Number);

            _buildingRepositoryMock.Verify(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _buildingRepositoryMock.Verify(r => r.Update(It.Is<Domain.Entities.Buildings.Building>(b => b == building)), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}