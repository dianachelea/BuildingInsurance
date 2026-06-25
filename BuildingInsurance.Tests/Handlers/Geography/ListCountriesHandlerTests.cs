using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Geography
{
    public class ListCountriesHandlerTests
    {
        private readonly Mock<ICountryRepository> _countryRepositoryMock;
        private readonly ListCountriesHandler _handler;

        public ListCountriesHandlerTests()
        {
            _countryRepositoryMock = new Mock<ICountryRepository>();
            _handler = new ListCountriesHandler(_countryRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoCountriesFound()
        {
            var query = new ListCountriesQuery{ Page = 1, PageSize = 10 };

            _countryRepositoryMock
                .Setup(r => r.GetAllCountriesPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Geography.Country>(), 0));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);

            _countryRepositoryMock.Verify(r => r.GetAllCountriesPagedAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedItems_Correctly_AndSortedByName()
        {
            var query = new ListCountriesQuery { Page = 2, PageSize = 2 };

            var c1 = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            var c2 = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Austria");
            var c3 = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Bulgaria");
            var c4 = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Denmark");
            var c5 = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Estonia");

            _countryRepositoryMock
                .Setup(r => r.GetAllCountriesPagedAsync(2, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new[] { c1, c2 }, 5));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("ROMANIA", result.Value.Items[0].Name);
            Assert.Equal("AUSTRIA", result.Value.Items[1].Name);

            _countryRepositoryMock.Verify(r => r.GetAllCountriesPagedAsync(2, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}