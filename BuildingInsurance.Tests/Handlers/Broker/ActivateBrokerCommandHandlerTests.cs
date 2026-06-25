using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.ActivateBroker;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Broker
{
    public class ActivateBrokerCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBrokerRepository> _brokerRepoMock;
        private readonly Mock<ILogger<ActivateBrokerCommandHandler>> _loggerMock;

        private readonly ActivateBrokerCommandHandler _handler;

        public ActivateBrokerCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _brokerRepoMock = new Mock<IBrokerRepository>();
            _loggerMock = new Mock<ILogger<ActivateBrokerCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Brokers).Returns(_brokerRepoMock.Object);

            _handler = new ActivateBrokerCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBrokerDoesNotExist()
        {
            var brokerId = Guid.NewGuid();
            var cmd = new ActivateBrokerCommand(brokerId);

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(brokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Management.Broker?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Broker with ID {brokerId} not found.", result.Error);

            _brokerRepoMock.Verify(r => r.GetByIdAsync(brokerId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldActivateAndCommit_WhenBrokerIsNotActive()
        {
            var cmd = new ActivateBrokerCommand(Guid.NewGuid());

            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "John Broker",
                contactInfo: new ContactInfo("john@x.com", "0712345678"),
                brokerStatus: BrokerStatus.Inactive,
                commissionPercentage: 0.2m);

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(BrokerStatus.Active, result.Value!.BrokerStatus);

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotCommit_WhenBrokerAlreadyActive()
        {
            var cmd = new ActivateBrokerCommand(Guid.NewGuid());

            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "John Broker",
                contactInfo: new ContactInfo("john@x.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.2m);

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(BrokerStatus.Active, result.Value!.BrokerStatus);

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenCommitThrows()
        {
            var cmd = new ActivateBrokerCommand(Guid.NewGuid());

            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "John Broker",
                contactInfo: new ContactInfo("john@x.com", "0712345678"),
                brokerStatus: BrokerStatus.Inactive,
                commissionPercentage: 0.2m);

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(cmd.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error during broker activation."));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during broker activation.", result.Error);

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}