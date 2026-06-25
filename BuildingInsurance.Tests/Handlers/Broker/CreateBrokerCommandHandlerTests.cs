using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.CreateBroker;
using BuildingInsurance.Application.Features.Common.Result;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Broker
{
    public class CreateBrokerCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBrokerRepository> _brokerRepoMock;
        private readonly Mock<ILogger<CreateBrokerCommandHandler>> _loggerMock;

        private readonly CreateBrokerCommandHandler _handler;

        public CreateBrokerCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _brokerRepoMock = new Mock<IBrokerRepository>();
            _loggerMock = new Mock<ILogger<CreateBrokerCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Brokers).Returns(_brokerRepoMock.Object);

            _handler = new CreateBrokerCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenBrokerCodeAlreadyExists()
        {
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.2m
            };

            _brokerRepoMock
                .Setup(r => r.BrokerCodeExistsAsync(cmd.BrokerCode!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another broker already exists with the same broker code.", result.Error);

            _brokerRepoMock.Verify(r => r.BrokerCodeExistsAsync(cmd.BrokerCode!, It.IsAny<CancellationToken>()), Times.Once);
            _brokerRepoMock.Verify(r => r.BrokerEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _brokerRepoMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Management.Broker>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.2m
            };

            _brokerRepoMock
                .Setup(r => r.BrokerCodeExistsAsync(cmd.BrokerCode!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _brokerRepoMock
                .Setup(r => r.BrokerEmailExistsAsync(cmd.Email!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another broker already exists with the same email address.", result.Error);

            _brokerRepoMock.Verify(r => r.BrokerCodeExistsAsync(cmd.BrokerCode!, It.IsAny<CancellationToken>()), Times.Once);
            _brokerRepoMock.Verify(r => r.BrokerEmailExistsAsync(cmd.Email!, It.IsAny<CancellationToken>()), Times.Once);
            _brokerRepoMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Management.Broker>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenValidInput()
        {
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.2m
            };

            _brokerRepoMock
                .Setup(r => r.BrokerCodeExistsAsync(cmd.BrokerCode!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _brokerRepoMock
                .Setup(r => r.BrokerEmailExistsAsync(cmd.Email!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(cmd.BrokerCode, result.Value!.BrokerCode);
            Assert.Equal(cmd.FullName, result.Value.FullName);
            Assert.Equal(cmd.Email, result.Value.Email);
            Assert.Equal(cmd.Phone, result.Value.Phone);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _brokerRepoMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Management.Broker>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldRollback_WhenExceptionOccurs_AfterTransactionStarts()
        {
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john@x.com",
                Phone = "0712345678",
                CommissionPercentage = 0.2m
            };

            _brokerRepoMock
                .Setup(r => r.BrokerCodeExistsAsync(cmd.BrokerCode!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _brokerRepoMock
                .Setup(r => r.BrokerEmailExistsAsync(cmd.Email!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _brokerRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Management.Broker>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("insert failed"));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Generic, result.ErrorType);
            Assert.Equal("Unexpected error during broker creation.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}