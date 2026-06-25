using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.UpdateBroker;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Broker
{
    public class UpdateBrokerCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBrokerRepository> _brokerRepoMock;
        private readonly Mock<ILogger<UpdateBrokerCommandHandler>> _loggerMock;

        private readonly UpdateBrokerCommandHandler _handler;

        public UpdateBrokerCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _brokerRepoMock = new Mock<IBrokerRepository>();
            _loggerMock = new Mock<ILogger<UpdateBrokerCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Brokers).Returns(_brokerRepoMock.Object);

            _handler = new UpdateBrokerCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBrokerDoesNotExist()
        {
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "New Name",
                Email = "new@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Management.Broker?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Broker with ID {cmd.Id} not found.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenEmailChanged_AndDuplicateExists()
        {
            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "Old Name",
                contactInfo: new ContactInfo("old@x.com", "0700000000"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.2m);

            var cmd = new UpdateBrokerCommand
            {
                Id = broker.Id,
                FullName = "New Name",
                Email = "new@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            _brokerRepoMock
                .Setup(r => r.BrokerEmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another broker already exists with the same email address.", result.Error);

            _brokerRepoMock.Verify(r => r.BrokerEmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldNotCheckDuplicateEmail_WhenEmailUnchanged_AfterNormalization()
        {
            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "Old Name",
                contactInfo: new ContactInfo("same@x.com", "0700000000"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.2m);

            var cmd = new UpdateBrokerCommand
            {
                Id = broker.Id,
                FullName = "New Name",
                Email = "  SAME@X.COM",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);

            _brokerRepoMock.Verify(r => r.BrokerEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _brokerRepoMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Management.Broker>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenValidInput()
        {
            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "Old Name",
                contactInfo: new ContactInfo("old@x.com", "0700000000"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.2m);

            var cmd = new UpdateBrokerCommand
            {
                Id = broker.Id,
                FullName = "New Name",
                Email = "new@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            _brokerRepoMock
                .Setup(r => r.BrokerEmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(cmd.FullName, result.Value!.FullName);
            Assert.Equal(cmd.Email.Trim().ToLowerInvariant(), result.Value.Email);
            Assert.Equal(cmd.Phone, result.Value.Phone);
            Assert.Equal(cmd.CommissionPercentage, result.Value.CommissionPercentage);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _brokerRepoMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Management.Broker>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldRollback_WhenExceptionOccurs_AfterTransactionStarts()
        {
            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "Old Name",
                contactInfo: new ContactInfo("old@x.com", "0700000000"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.2m);

            var cmd = new UpdateBrokerCommand
            {
                Id = broker.Id,
                FullName = "New Name",
                Email = "new@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            _brokerRepoMock
                .Setup(r => r.BrokerEmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error while updating broker."));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error while updating broker.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}