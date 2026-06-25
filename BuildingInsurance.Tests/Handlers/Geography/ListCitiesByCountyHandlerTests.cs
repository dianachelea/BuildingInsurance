using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Geography
{
    public class ListCitiesByCountyHandlerTests
    {
        private readonly Mock<ICityRepository> _cityRepositoryMock;
        private readonly ListCitiesByCountyHandler _handler;

        public ListCitiesByCountyHandlerTests()
        {
            _cityRepositoryMock = new Mock<ICityRepository>();
            _handler = new ListCitiesByCountyHandler(_cityRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoCitiesFound()
        {
            var countyId = Guid.NewGuid();
            var query = new ListCitiesByCountyQuery{ CountyId = countyId, Page = 1, PageSize = 10 };

            _cityRepositoryMock
                .Setup(r => r.GetByCountyIdPagedAsync(countyId, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Geography.City>(), 0));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);

            _cityRepositoryMock.Verify(r => r.GetByCountyIdPagedAsync(countyId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedItems_Correctly_AndSortedByName()
        {
            var countyId = Guid.NewGuid();
            var query = new ListCitiesByCountyQuery{ CountyId = countyId, Page = 2, PageSize = 2 };

            var city1 = new Domain.Entities.Geography.City(Guid.NewGuid(), "CharlieTown", countyId);
            var city2 = new Domain.Entities.Geography.City(Guid.NewGuid(), "AliceTown", countyId);
            var city3 = new Domain.Entities.Geography.City(Guid.NewGuid(), "BobTown", countyId);
            var city4 = new Domain.Entities.Geography.City(Guid.NewGuid(), "DavidTown", countyId);
            var city5 = new Domain.Entities.Geography.City(Guid.NewGuid(), "EveTown", countyId);

            _cityRepositoryMock
                .Setup(r => r.GetByCountyIdPagedAsync(countyId, 2, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new[] { city3, city4 }, 5));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("BOBTOWN", result.Value.Items[0].Name);
            Assert.Equal("DAVIDTOWN", result.Value.Items[1].Name);

            _cityRepositoryMock.Verify(r => r.GetByCountyIdPagedAsync(countyId, 2, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}