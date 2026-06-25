using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.GetCurrencyById;
using BuildingInsurance.Application.Features.Common.Result;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Currency
{
    public class GetCurrencyByIdHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;

        private readonly GetCurrencyByIdHandler _handler;

        public GetCurrencyByIdHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currencyRepositoryMock = new Mock<ICurrencyRepository>();
            _unitOfWorkMock.SetupGet(u => u.Currencies).Returns(_currencyRepositoryMock.Object);

            _handler = new GetCurrencyByIdHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenCurrencyDoesNotExist()
        {
            var id = Guid.NewGuid();
            var query = new GetCurrencyByIdQuery(id);

            _currencyRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Metadata.Currency?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Currency not found.", result.Error);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenCurrencyExists()
        {
            var currency = new Domain.Entities.Metadata.Currency("EUR", "Euro", 4.95m, true);
            var query = new GetCurrencyByIdQuery(currency.Id);

            _currencyRepositoryMock
                .Setup(r => r.GetByIdAsync(currency.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currency);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(currency.Id, result.Value!.Id);
            Assert.Equal(currency.Code, result.Value.Code);
            Assert.Equal(currency.Name, result.Value.Name);
            Assert.Equal(currency.ExchangeRateToBase, result.Value.ExchangeRateToBase);
            Assert.Equal(currency.IsActive, result.Value.IsActive);
        }
    }
}