using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Domain.Enums;
using Moq;

namespace BuildingInsurance.Tests.Handlers.RiskFactorConfiguration
{
    public sealed class ListRiskFactorConfigurationsHandlerTests
    {
        private readonly Mock<IRiskFactorConfigurationRepository> _repoMock;
        private readonly ListRiskFactorConfigurationsHandler _handler;

        public ListRiskFactorConfigurationsHandlerTests()
        {
            _repoMock = new Mock<IRiskFactorConfigurationRepository>();
            _handler = new ListRiskFactorConfigurationsHandler(_repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoItemsFound()
        {
            var query = new ListRiskFactorConfigurationsQuery{ Level = null, ReferenceId = null, IsActive = null, Page = 1, PageSize = 10 };

            _repoMock
                .Setup(r => r.SearchPagedAsync(
                    query.Level.MapToDomainRiskFactorLevelOptional(), query.ReferenceId, query.IsActive,
                    query.Page, query.PageSize,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Metadata.RiskFactorConfiguration>(), 0));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);

            _repoMock.Verify(r => r.SearchPagedAsync(
                query.Level.MapToDomainRiskFactorLevelOptional(), query.ReferenceId, query.IsActive,
                1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenPageIsGreaterThanTotalPages()
        {
            var query = new ListRiskFactorConfigurationsQuery{ Level = null, ReferenceId = null, IsActive = null, Page = 3, PageSize = 2 };

            _repoMock
                .Setup(r => r.SearchPagedAsync(
                    query.Level.MapToDomainRiskFactorLevelOptional(), query.ReferenceId, query.IsActive,
                    query.Page, query.PageSize,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Array.Empty<Domain.Entities.Metadata.RiskFactorConfiguration>(), 3));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(3, result.Value.TotalCount);
            Assert.Equal(2, result.Value.TotalPages);

            _repoMock.Verify(r => r.SearchPagedAsync(
                query.Level.MapToDomainRiskFactorLevelOptional(), query.ReferenceId, query.IsActive,
                3, 2, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedItems_Correctly()
        {
            var refFilter = Guid.NewGuid();

            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = refFilter,
                IsActive = true,
                Page = 2,
                PageSize = 2
            };

            var rf1 = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var rf2 = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.20m,
                isActive: true);

            _repoMock
                .Setup(r => r.SearchPagedAsync(
                    query.Level.MapToDomainRiskFactorLevelOptional(),
                    query.ReferenceId,
                    query.IsActive,
                    query.Page,
                    query.PageSize,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new[] { rf1, rf2 }, 5));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal(rf1.Id, result.Value.Items[0].Id);
            Assert.Equal(rf1.Level, result.Value.Items[0].Level);
            Assert.Equal(rf1.ReferenceId, result.Value.Items[0].ReferenceId);
            Assert.Equal(rf1.BuildingType, result.Value.Items[0].BuildingType);

            Assert.Equal(rf2.Id, result.Value.Items[1].Id);
            Assert.Equal(rf2.Level, result.Value.Items[1].Level);
            Assert.Equal(rf2.ReferenceId, result.Value.Items[1].ReferenceId);
            Assert.Equal(rf2.BuildingType, result.Value.Items[1].BuildingType);

            _repoMock.Verify(r => r.SearchPagedAsync(
                query.Level.MapToDomainRiskFactorLevelOptional(),
                query.ReferenceId,
                query.IsActive,
                2,
                2,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
