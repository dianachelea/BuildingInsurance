using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Geography
{
    public class ListCountiesByCountryHandlerTests
    {
        private readonly Mock<ICountyRepository> _countyRepositoryMock;
        private readonly ListCountiesByCountryHandler _handler;

        public ListCountiesByCountryHandlerTests()
        {
            _countyRepositoryMock = new Mock<ICountyRepository>();
            _handler = new ListCountiesByCountryHandler(_countyRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoCountiesFound()
        {
            var countryId = Guid.NewGuid();
            var query = new ListCountiesByCountryQuery{ CountryId = countryId, Page = 1, PageSize = 10 };

            _countyRepositoryMock
                .Setup(r => r.GetByCountryIdPagedAsync(countryId, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Geography.County>(), 0));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);

            _countyRepositoryMock.Verify(r => r.GetByCountryIdPagedAsync(countryId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedItems_Correctly_AndSortedByName()
        {
            var countryId = Guid.NewGuid();
            var query = new ListCountiesByCountryQuery { CountryId = countryId, Page = 2, PageSize = 2 };

            var c1 = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", countryId);
            var c2 = new Domain.Entities.Geography.County(Guid.NewGuid(), "Alba", countryId);
            var c3 = new Domain.Entities.Geography.County(Guid.NewGuid(), "Bihor", countryId);
            var c4 = new Domain.Entities.Geography.County(Guid.NewGuid(), "Dolj", countryId);
            var c5 = new Domain.Entities.Geography.County(Guid.NewGuid(), "Eforie", countryId);

            _countyRepositoryMock
                .Setup(r => r.GetByCountryIdPagedAsync(countryId, 2, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new[] { c3, c4 }, 5));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("BIHOR", result.Value.Items[0].Name);
            Assert.Equal("DOLJ", result.Value.Items[1].Name);

            _countyRepositoryMock.Verify(r => r.GetByCountryIdPagedAsync(countryId, 2, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}