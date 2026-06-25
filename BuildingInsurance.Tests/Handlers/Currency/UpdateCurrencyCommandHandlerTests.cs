using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.UpdateCurrency;
using BuildingInsurance.Application.Features.Common.Result;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Currency
{
    public class UpdateCurrencyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
        private readonly Mock<ILogger<UpdateCurrencyCommandHandler>> _loggerMock;

        private readonly UpdateCurrencyCommandHandler _handler;

        public UpdateCurrencyCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currencyRepositoryMock = new Mock<ICurrencyRepository>();
            _loggerMock = new Mock<ILogger<UpdateCurrencyCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Currencies).Returns(_currencyRepositoryMock.Object);

            _handler = new UpdateCurrencyCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenCurrencyDoesNotExist()
        {
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = "Euro",
                ExchangeRateToBase = 4.95m,
                IsActive = true
            };

            _currencyRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Metadata.Currency?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Currency with ID {cmd.Id} not found.", result.Error);

            _currencyRepositoryMock.Verify(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenTryingToDeactivate_AndCurrencyUsedInActivePolicies()
        {
            var id = Guid.NewGuid();
            var existing = new Domain.Entities.Metadata.Currency("EUR", "Euro", 4.9m, isActive: true);

            var cmd = new UpdateCurrencyCommand
            {
                Id = id,
                Name = "Euro Updated",
                ExchangeRateToBase = 5.0m,
                IsActive = false
            };

            _currencyRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _currencyRepositoryMock
                .Setup(r => r.IsUsedInActivePoliciesAsync(existing.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Cannot deactivate currency which is used in active policies.", result.Error);

            _currencyRepositoryMock.Verify(r => r.IsUsedInActivePoliciesAsync(existing.Id, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
            _currencyRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Metadata.Currency>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenValidUpdate()
        {
            var existing = new Domain.Entities.Metadata.Currency("EUR", "Euro", 4.9m, isActive: false);

            var cmd = new UpdateCurrencyCommand
            {
                Id = existing.Id,
                Name = "Euro Updated",
                ExchangeRateToBase = 5.0m,
                IsActive = true
            };

            _currencyRepositoryMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _unitOfWorkMock
                .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(existing.Id, result.Value!.Id);
            Assert.Equal(existing.Code, result.Value.Code);
            Assert.Equal("Euro Updated", result.Value.Name);
            Assert.Equal(5.0m, result.Value.ExchangeRateToBase);
            Assert.True(result.Value.IsActive);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _currencyRepositoryMock.Verify(r => r.Update(It.Is<Domain.Entities.Metadata.Currency>(c =>
                c.Id == existing.Id &&
                c.Name == "Euro Updated" &&
                c.ExchangeRateToBase == 5.0m &&
                c.IsActive == true
            )), Times.Once);

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}