using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Broker
{
    public class ListBrokersHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBrokerRepository> _brokerRepoMock;
        private readonly ListBrokersHandler _handler;

        public ListBrokersHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _brokerRepoMock = new Mock<IBrokerRepository>();
            _unitOfWorkMock.SetupGet(u => u.Brokers).Returns(_brokerRepoMock.Object);

            _handler = new ListBrokersHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoBrokersFound()
        {
            var query = new ListBrokersQuery
            {
                Name = null,
                IsActive = null,
                Page = 1,
                PageSize = 10
            };

            _brokerRepoMock
                .Setup(r => r.SearchPagedAsync(query.Name, query.IsActive, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Management.Broker>(), 0));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);

            _brokerRepoMock.Verify(r => r.SearchPagedAsync(query.Name, query.IsActive, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldComputeTotalPages_Correctly()
        {
            var query = new ListBrokersQuery
            {
                Name = null,
                IsActive = null,
                Page = 2,
                PageSize = 2
            };

            var b1 = new Domain.Entities.Management.Broker("BR01", "Alice", new ContactInfo("a@x.com", "0700"), BrokerStatus.Active, 0.2m);
            var b2 = new Domain.Entities.Management.Broker("BR02", "Bob", new ContactInfo("b@x.com", "0700"), BrokerStatus.Inactive, 0.3m);

            _brokerRepoMock
                .Setup(r => r.SearchPagedAsync(query.Name, query.IsActive, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new[] { b1, b2 }, 5));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("Alice", result.Value.Items[0].FullName);
            Assert.Equal("Bob", result.Value.Items[1].FullName);

            _brokerRepoMock.Verify(r => r.SearchPagedAsync(query.Name, query.IsActive, 2, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}