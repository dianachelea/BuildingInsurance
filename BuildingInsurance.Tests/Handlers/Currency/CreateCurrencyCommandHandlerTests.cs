using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.CreateCurrency;
using BuildingInsurance.Application.Features.Common.Result;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Currency
{
    public class CreateCurrencyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
        private readonly Mock<ILogger<CreateCurrencyCommandHandler>> _loggerMock;

        private readonly CreateCurrencyCommandHandler _handler;

        public CreateCurrencyCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currencyRepositoryMock = new Mock<ICurrencyRepository>();
            _loggerMock = new Mock<ILogger<CreateCurrencyCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Currencies).Returns(_currencyRepositoryMock.Object);

            _handler = new CreateCurrencyCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenCurrencyCodeAlreadyExists()
        {
            var cmd = new CreateCurrencyCommand
            {
                Code = "EUR",
                Name = "Euro",
                ExchangeRateToBase = 4.95m,
                IsActive = true
            };

            _currencyRepositoryMock
                .Setup(r => r.GetByCodeAsync(cmd.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Metadata.Currency(cmd.Code, "Existing", 1m, true));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Currency code already exists.", result.Error);

            _currencyRepositoryMock.Verify(r => r.GetByCodeAsync(cmd.Code, It.IsAny<CancellationToken>()), Times.Once);
            _currencyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Metadata.Currency>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenValidInput()
        {
            var cmd = new CreateCurrencyCommand
            {
                Code = "EUR",
                Name = "Euro",
                ExchangeRateToBase = 4.95m,
                IsActive = true
            };

            _currencyRepositoryMock
                .Setup(r => r.GetByCodeAsync(cmd.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Metadata.Currency?)null);

            _currencyRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Metadata.Currency>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);
            Assert.Equal(cmd.Code, result.Value.Code);
            Assert.Equal(cmd.Name, result.Value.Name);
            Assert.Equal(cmd.ExchangeRateToBase, result.Value.ExchangeRateToBase);
            Assert.Equal(cmd.IsActive, result.Value.IsActive);

            _currencyRepositoryMock.Verify(r => r.GetByCodeAsync(cmd.Code, It.IsAny<CancellationToken>()), Times.Once);
            _currencyRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Entities.Metadata.Currency>(c =>
                c.Code == cmd.Code &&
                c.Name == cmd.Name &&
                c.ExchangeRateToBase == cmd.ExchangeRateToBase &&
                c.IsActive == cmd.IsActive
            ), It.IsAny<CancellationToken>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}