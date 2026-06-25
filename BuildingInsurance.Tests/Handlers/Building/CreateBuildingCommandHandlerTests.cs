using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Geography;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.CreateBuilding;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Application.Features.Common.Mapping;

namespace BuildingInsurance.Tests.Handlers.Building
{
    public class CreateBuildingCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly Mock<IBuildingRepository> _buildingRepositoryMock;

        private readonly Mock<ILogger<CreateBuildingCommandHandler>> _loggerMock;

        private readonly CreateBuildingCommandHandler _handler;

        public CreateBuildingCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _cityRepositoryMock = new Mock<ICityRepository>();
            _buildingRepositoryMock = new Mock<IBuildingRepository>();

            _loggerMock = new Mock<ILogger<CreateBuildingCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Clients).Returns(_clientRepositoryMock.Object);
            _unitOfWorkMock.SetupGet(u => u.Cities).Returns(_cityRepositoryMock.Object);
            _unitOfWorkMock.SetupGet(u => u.Buildings).Returns(_buildingRepositoryMock.Object);

            _handler = new CreateBuildingCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Client_DoesNotExist()
        {
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.5m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Clients.Client?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Client with ID {cmd.ClientId} not found.", result.Error);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(cmd.ClientId, It.IsAny<CancellationToken>()), Times.Once);

            _cityRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _buildingRepositoryMock.Verify(r => r.ExistsForClientAtAddressAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _buildingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<BuildingInsurance.Domain.Entities.Buildings.Building>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_City_DoesNotExist()
        {
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.5m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Clients.Client(cmd.ClientId, ClientType.Individual, "John Doe", new ContactInfo("john@email.com", "0771123298"), "12345678901"));

            _cityRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.CityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((City?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"City with ID {cmd.CityId} not found.", result.Error);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(cmd.ClientId, It.IsAny<CancellationToken>()), Times.Once);
            _cityRepositoryMock.Verify(r => r.GetByIdAsync(cmd.CityId, It.IsAny<CancellationToken>()), Times.Once);

            _buildingRepositoryMock.Verify(r => r.ExistsForClientAtAddressAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _buildingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<BuildingInsurance.Domain.Entities.Buildings.Building>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_When_Building_AlreadyExists_For_Client_At_Address()
        {
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.5m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Clients.Client(
                    cmd.ClientId,
                    ClientType.Individual,
                    "John Doe",
                    new ContactInfo("john@email.com", "0771123298"),
                    "12345678901"
                ));

            _cityRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.CityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new City(cmd.CityId, "Bucharest", Guid.NewGuid()));

            _buildingRepositoryMock
                .Setup(r => r.ExistsForClientAtAddressAsync(cmd.ClientId, cmd.CityId, cmd.Street, cmd.Number, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Building already exists at the specified address for this client.", result.Error);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(cmd.ClientId, It.IsAny<CancellationToken>()), Times.Once);
            _cityRepositoryMock.Verify(r => r.GetByIdAsync(cmd.CityId, It.IsAny<CancellationToken>()), Times.Once);
            _buildingRepositoryMock.Verify(r => r.ExistsForClientAtAddressAsync(cmd.ClientId, cmd.CityId, cmd.Street, cmd.Number, It.IsAny<CancellationToken>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            _buildingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Buildings.Building>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_When_Input_Is_Valid()
        {
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.5m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Clients.Client(
                    cmd.ClientId,
                    ClientType.Individual,
                    "John Doe",
                    new ContactInfo("john@email.com", "0771123298"),
                    "12345678901"
                ));

            _cityRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.CityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new City(cmd.CityId, "Bucharest", Guid.NewGuid()));

            _buildingRepositoryMock
                .Setup(r => r.ExistsForClientAtAddressAsync(cmd.ClientId, cmd.CityId, cmd.Street, cmd.Number, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            Assert.NotEqual(Guid.Empty, result.Value!.Id);
            Assert.Equal(cmd.ClientId, result.Value.ClientId);
            Assert.Equal(cmd.CityId, result.Value.CityId);
            Assert.Equal(cmd.Street.ToUpperInvariant(), result.Value.Street);
            Assert.Equal(cmd.Number, result.Value.Number);
            Assert.Equal(cmd.ConstructionYear, result.Value.ConstructionYear);
            Assert.Equal(cmd.Type.MapToDomainBuildingType(), result.Value.Type);
            Assert.Equal(cmd.NumberOfFloors, result.Value.NumberOfFloors);
            Assert.Equal(cmd.SurfaceArea, result.Value.SurfaceArea);
            Assert.Equal(cmd.InsuredValue, result.Value.InsuredValue);
            Assert.Equal(cmd.RiskIndicators.MapToDomainRiskIndicators(), result.Value.RiskIndicators);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            _buildingRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Entities.Buildings.Building>(b =>
                b.ClientId == cmd.ClientId &&
                b.CityId == cmd.CityId &&
                b.Address.Street == cmd.Street.ToUpperInvariant() &&
                b.Address.Number == cmd.Number &&
                b.ConstructionYear == cmd.ConstructionYear &&
                b.Type == cmd.Type.MapToDomainBuildingType() &&
                b.NumberOfFloors == cmd.NumberOfFloors &&
                b.SurfaceArea == cmd.SurfaceArea &&
                b.InsuredValue == cmd.InsuredValue &&
                b.RiskIndicators == cmd.RiskIndicators.MapToDomainRiskIndicators()
            ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}