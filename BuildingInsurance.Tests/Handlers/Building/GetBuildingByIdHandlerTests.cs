using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using Moq;
using BuildingInsurance.Application.Features.Brokers.Buildings.Queries.GetBuildingById;

namespace BuildingInsurance.Tests.Handlers.Building
{
    public class GetBuildingByIdHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBuildingRepository> _buildingRepositoryMock;
        private readonly Mock<IGeographyCachingService> _geographyMock;
        private readonly GetBuildingByIdHandler _handler;

        public GetBuildingByIdHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _buildingRepositoryMock = new Mock<IBuildingRepository>();
            _geographyMock = new Mock<IGeographyCachingService>();

            _unitOfWorkMock
                .Setup(u => u.Buildings)
                .Returns(_buildingRepositoryMock.Object);

            _handler = new GetBuildingByIdHandler(
                _unitOfWorkMock.Object,
                _geographyMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBuildingDoesNotExist()
        {
            var buildingId = Guid.NewGuid();
            var query = new GetBuildingByIdQuery(buildingId);

            _buildingRepositoryMock
                .Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Buildings.Building?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Building with ID {buildingId} not found.", result.Error);

            _buildingRepositoryMock.Verify(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGeographyNotFound()
        {
            var buildingId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var query = new GetBuildingByIdQuery(buildingId);

            var address = new Address(street: "Main St", number: "10");

            var riskIndicators = RiskIndicators.EarthquakeProne;

            var building = new Domain.Entities.Buildings.Building(
                id: buildingId,
                clientId: Guid.NewGuid(),
                address: address,
                cityId: cityId,
                constructionYear: 2005,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150_000m,
                riskIndicators: riskIndicators);

            _buildingRepositoryMock
                .Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            _geographyMock
                .Setup(g => g.TryGet(
                    cityId,
                    out It.Ref<string>.IsAny,
                    out It.Ref<string>.IsAny,
                    out It.Ref<string>.IsAny))
                .Returns(false);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Geography not found for building.", result.Error);

            _buildingRepositoryMock.Verify(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()), Times.Once);
            _geographyMock.Verify(g => g.TryGet(cityId, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenBuildingAndGeographyExist()
        {
            var buildingId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var query = new GetBuildingByIdQuery(buildingId);

            var address = new Address(street: "Unirii", number: "5");

            var riskIndicators = new RiskIndicators();

            var building = new Domain.Entities.Buildings.Building(
                id: buildingId,
                clientId: clientId,
                address: address,
                cityId: cityId,
                constructionYear: 1998,
                type: BuildingType.Industrial,
                numberOfFloors: 5,
                surfaceArea: 350m,
                insuredValue: 500_000m,
                riskIndicators: riskIndicators);

            _buildingRepositoryMock
                .Setup(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);

            string city = "Cluj-Napoca";
            string county = "Cluj";
            string country = "Romania";

            _geographyMock
                .Setup(g => g.TryGet(cityId, out city, out county, out country))
                .Returns(true);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var dto = result.Value!;

            Assert.Equal(buildingId, dto.Id);
            Assert.Equal(clientId, dto.ClientId);
            Assert.Equal("UNIRII", dto.Street.ToUpperInvariant());
            Assert.Equal("5", dto.Number);
            Assert.Equal("CLUJ-NAPOCA", dto.City.ToUpperInvariant());
            Assert.Equal("CLUJ", dto.County.ToUpperInvariant());
            Assert.Equal("ROMANIA", dto.Country.ToUpperInvariant());
            Assert.Equal(1998, dto.ConstructionYear);
            Assert.Equal(BuildingType.Industrial, dto.Type);
            Assert.Equal(5, dto.NumberOfFloors);
            Assert.Equal(350m, dto.SurfaceArea);
            Assert.Equal(500_000m, dto.InsuredValue);

            _buildingRepositoryMock.Verify(r => r.GetByIdAsync(buildingId, It.IsAny<CancellationToken>()), Times.Once);
            _geographyMock.Verify(g => g.TryGet(cityId, out city, out county, out country), Times.Once);
        }
    }
}