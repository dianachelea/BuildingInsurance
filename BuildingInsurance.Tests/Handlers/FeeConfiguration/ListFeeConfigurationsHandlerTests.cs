using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations;
using BuildingInsurance.Domain.Enums;
using Moq;

namespace BuildingInsurance.Tests.Handlers.FeeConfiguration
{
    public sealed class ListFeeConfigurationsHandlerTests
    {
        private readonly Mock<IFeeConfigurationRepository> _feeRepoMock;
        private readonly ListFeeConfigurationsHandler _handler;

        public ListFeeConfigurationsHandlerTests()
        {
            _feeRepoMock = new Mock<IFeeConfigurationRepository>();
            _handler = new ListFeeConfigurationsHandler(_feeRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoItemsFound()
        {
            var query = new ListFeeConfigurationsQuery{ Name = null, Type = null, IsActive = null, Page = 1, PageSize = 10 };

            _feeRepoMock
                .Setup(r => r.SearchPagedAsync(query.Name, (FeeType?)null, query.IsActive, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Metadata.FeeConfiguration>(), 0));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);

            _feeRepoMock.Verify(r => r.SearchPagedAsync(query.Name, (FeeType?)null, query.IsActive, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenPageIsGreaterThanTotalPages()
        {
            var query = new ListFeeConfigurationsQuery{ Name = null, Type = null, IsActive = null, Page = 3, PageSize = 2 };

            _feeRepoMock
                .Setup(r => r.SearchPagedAsync(query.Name, (FeeType?)null, query.IsActive, 3, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Metadata.FeeConfiguration>(), 3));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(3, result.Value.TotalCount);
            Assert.Equal(2, result.Value.TotalPages);

            _feeRepoMock.Verify(r => r.SearchPagedAsync(query.Name, (FeeType?)null, query.IsActive, 3, 2, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedItems_Correctly()
        {
            var query = new ListFeeConfigurationsQuery{ Name = null, Type = null, IsActive = null, Page = 2, PageSize = 2 };

            var fee1 = new BuildingInsurance.Domain.Entities.Metadata.FeeConfiguration(
                "Admin Fee",
                FeeType.AdminFee,
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30),
                true,
                RiskIndicators.None);

            var fee2 = new BuildingInsurance.Domain.Entities.Metadata.FeeConfiguration(
                "Broker Commission",
                FeeType.BrokerCommission,
                0.05m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30),
                true,
                RiskIndicators.None);

            _feeRepoMock
                .Setup(r => r.SearchPagedAsync(query.Name, (FeeType?)null, query.IsActive, 2, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new[] { fee1, fee2 }, 5));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("Admin Fee", result.Value.Items[0].Name);
            Assert.Equal("Broker Commission", result.Value.Items[1].Name);

            _feeRepoMock.Verify(r => r.SearchPagedAsync(query.Name, (FeeType?)null, query.IsActive, 2, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}