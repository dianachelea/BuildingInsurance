using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Queries.GetBrokerById;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Broker
{
    public class GetBrokerByIdHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBrokerRepository> _brokerRepoMock;
        private readonly GetBrokerByIdHandler _handler;

        public GetBrokerByIdHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _brokerRepoMock = new Mock<IBrokerRepository>();
            _unitOfWorkMock.SetupGet(u => u.Brokers).Returns(_brokerRepoMock.Object);

            _handler = new GetBrokerByIdHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenBrokerDoesNotExist()
        {
            var brokerId = Guid.NewGuid();
            var query = new GetBrokerByIdQuery(brokerId);

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(brokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Management.Broker?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Broker with ID {brokerId} not found.", result.Error);

            _brokerRepoMock.Verify(r => r.GetByIdAsync(brokerId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenBrokerExists()
        {
            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BR01",
                name: "John Broker",
                contactInfo: new ContactInfo("john@x.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.2m);

            var query = new GetBrokerByIdQuery(broker.Id);

            _brokerRepoMock
                .Setup(r => r.GetByIdAsync(broker.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(broker);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(broker.Id, result.Value!.Id);
            Assert.Equal("BR01", result.Value.BrokerCode);
            Assert.Equal("John Broker", result.Value.FullName);
            Assert.Equal("john@x.com", result.Value.Email);
            Assert.Equal("0712345678", result.Value.Phone);

            _brokerRepoMock.Verify(r => r.GetByIdAsync(broker.Id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}