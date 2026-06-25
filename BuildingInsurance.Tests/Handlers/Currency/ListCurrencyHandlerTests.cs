using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Currency
{
    public class ListCurrenciesHandlerTests
    {
        private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
        private readonly ListCurrenciesHandler _handler;

        public ListCurrenciesHandlerTests()
        {
            _currencyRepositoryMock = new Mock<ICurrencyRepository>();
            _handler = new ListCurrenciesHandler(_currencyRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WithTotalPagesZero_WhenNoResults()
        {
            var query = new ListCurrenciesQuery{ Name = null, IsActive = null, Page = 1, PageSize = 5 };

            _currencyRepositoryMock.Setup(r => r.SearchPagedAsync(query.Name, query.IsActive, query.Page, query.PageSize, CancellationToken.None))
                .ReturnsAsync((Array.Empty<Domain.Entities.Metadata.Currency>(), 0));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(0, result.Value!.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);
            Assert.Empty(result.Value.Items);
        }

        [Fact]
        public async Task Handle_ShouldCompute_TotalPages_Correctly()
        {
            var query = new ListCurrenciesQuery { Name = "E", IsActive = true, Page = 1, PageSize = 5 };

            var currencies = new List<Domain.Entities.Metadata.Currency>
            {
                new Domain.Entities.Metadata.Currency("EUR", "Euro", 4.95m, true),
                new Domain.Entities.Metadata.Currency("USD", "US Dollar", 4.6m, true),
            };

            _currencyRepositoryMock.Setup(r => r.SearchPagedAsync(query.Name, query.IsActive, query.Page, query.PageSize, CancellationToken.None))
                .ReturnsAsync((currencies, 12));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(12, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Contains(result.Value.Items, i => i.Code == "EUR" && i.Name == "Euro");
            Assert.Contains(result.Value.Items, i => i.Code == "USD" && i.Name == "US Dollar");

            _currencyRepositoryMock.Verify(r => r.SearchPagedAsync(query.Name, query.IsActive, query.Page, query.PageSize, CancellationToken.None), Times.Once);
        }
    }
}