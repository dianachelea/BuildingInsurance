using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.DeactivateBroker;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Broker
{
    public class DeactivateBrokerCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBrokerRepository> _brokerRepoMock;
        private readonly Mock<ILogger<DeactivateBrokerCommandHandler>> _loggerMock;

        private readonly DeactivateBrokerCommandHandler _handler;

        public DeactivateBrokerCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _brokerRepoMock = new Mock<IBrokerRepository>();
            _loggerMock = new Mock<ILogger<DeactivateBrokerCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Brokers).Returns(_brokerRepoMock.Object);

            _handler = new DeactivateBrokerCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBrokerDoesNotExist()
        {
            var brokerId = Guid.NewGuid();
            var cmd = new DeactivateBrokerCommand(brokerId);

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(brokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Management.Broker?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Broker with ID {brokerId} not found.", result.Error);

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldDeactivateAndCommit_WhenBrokerIsNotInactive()
        {
            var cmd = new DeactivateBrokerCommand(Guid.NewGuid());

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
            Assert.Equal(BrokerStatus.Inactive, result.Value!.BrokerStatus);

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotCommit_WhenBrokerAlreadyInactive()
        {
            var cmd = new DeactivateBrokerCommand(Guid.NewGuid());

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
            Assert.Equal(BrokerStatus.Inactive, result.Value!.BrokerStatus);

            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}