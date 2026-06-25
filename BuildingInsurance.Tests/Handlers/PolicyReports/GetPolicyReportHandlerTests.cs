using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies;
using BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.PolicyReports
{
    public sealed class GetPolicyReportHandlerTests
    {
        private readonly Mock<IPolicyReportStrategySelector> _selector = new();
        private readonly Mock<IPolicyReportStrategy> _strategy = new();
        private readonly Mock<ILogger<GetPolicyReportHandler>> _logger = new();

        private readonly GetPolicyReportHandler _handler;

        public GetPolicyReportHandlerTests()
        {
            _selector
                .Setup(s => s.Select(It.IsAny<ReportDimension>()))
                .Returns(_strategy.Object);

            _handler = new GetPolicyReportHandler(_selector.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_ShouldSelectStrategy_AndReturnRows()
        {
            var from = new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc);

            var filters = new PolicyReportFilters(
                From: from,
                To: to,
                Status: PolicyStatusContract.Active,
                CurrencyCode: "EUR",
                BuildingType: null);

            var expectedRows = new List<PolicyReportRowDto>
            {
                new("RO", "EUR", 2, 200m, 200m),
                new("BG", "EUR", 1, 120m, 120m),
            };

            _strategy
                .Setup(s => s.GenerateReportAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<PolicyStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<BuildingType?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRows);

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Country,
                Filters: filters);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            var response = result.Value!;
            Assert.Equal(expectedRows, response.Items);
            Assert.Equal(expectedRows.Count, response.TotalCount);
            Assert.Equal(1, response.TotalPages);

            _selector.Verify(s => s.Select(ReportDimension.Country), Times.Once);
            _strategy.Verify(s => s.GenerateReportAsync(
                It.Is<DateTime>(d => d == from.ToUniversalTime()),
                It.Is<DateTime>(d => d == to.ToUniversalTime()),
                It.IsAny<PolicyStatus>(),
                It.IsAny<string>(),
                It.IsAny<BuildingType?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPassMappedFilters_ToStrategy()
        {
            var from = new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 03, 31, 0, 0, 0, DateTimeKind.Utc);

            var filters = new PolicyReportFilters(
                From: from,
                To: to,
                Status: PolicyStatusContract.Active,
                CurrencyCode: "EUR",
                BuildingType: BuildingTypeContract.Residential);

            PolicyStatus? capturedStatus = null;
            string? capturedCurrency = null;
            BuildingType? capturedBuildingType = null;
            DateTime capturedFrom = default;
            DateTime capturedTo = default;

            _strategy
                .Setup(s => s.GenerateReportAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<PolicyStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<BuildingType?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<DateTime, DateTime, PolicyStatus, string, BuildingType?, CancellationToken>(
                    (f, t, st, cc, bt, ct) =>
                    {
                        capturedFrom = f;
                        capturedTo = t;
                        capturedStatus = st;
                        capturedCurrency = cc;
                        capturedBuildingType = bt;
                    })
                .ReturnsAsync(new List<PolicyReportRowDto>());

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.City,
                Filters: filters);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);

            Assert.Equal(PolicyStatus.Active, capturedStatus);
            Assert.Equal("EUR", capturedCurrency);
            Assert.Equal(BuildingType.Residential, capturedBuildingType);

            Assert.Equal(from.ToUniversalTime(), capturedFrom);
            Assert.Equal(to.ToUniversalTime(), capturedTo);

            _selector.Verify(s => s.Select(ReportDimension.City), Times.Once);
            _strategy.Verify(s => s.GenerateReportAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<PolicyStatus>(),
                It.IsAny<string>(),
                It.IsAny<BuildingType?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenStrategyReturnsEmpty()
        {
            var from = new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 02, 28, 0, 0, 0, DateTimeKind.Utc);

            var filters = new PolicyReportFilters(
                From: from,
                To: to,
                Status: PolicyStatusContract.Active,
                CurrencyCode: "EUR",
                BuildingType: null);

            _strategy
                .Setup(s => s.GenerateReportAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<PolicyStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<BuildingType?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PolicyReportRowDto>());

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Broker,
                Filters: filters);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            var response = result.Value!;
            Assert.Empty(response.Items);
            Assert.Equal(0, response.TotalCount);
            Assert.Equal(0, response.TotalPages);

            _selector.Verify(s => s.Select(ReportDimension.Broker), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenSelectorThrows_ShouldReturnFailureResult()
        {
            _selector
                .Setup(s => s.Select(It.IsAny<ReportDimension>()))
                .Throws(new InvalidOperationException("No report strategy registered."));

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.County,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 01, 31, 0, 0, 0, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.Equal("Unexpected error during report generating.", result.Error);
        }
    }
}