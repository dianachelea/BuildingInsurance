using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.FeeConfigurations
{
    public sealed class ListFeeConfigurationsIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListFeeConfigurationsHandler _handler;

        public ListFeeConfigurationsIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var feeRepo = new FeeConfigurationRepository(_db);
            _handler = new ListFeeConfigurationsHandler(feeRepo);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var f1 = new FeeConfiguration(
                feeName: "Admin Fee 2026",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var f2 = new FeeConfiguration(
                feeName: "Broker Commission 2026",
                feeType: FeeType.BrokerCommission,
                feePercentage: 0.15m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None);

            var f3 = new FeeConfiguration(
                feeName: "Risk Adj - Flood 2026",
                feeType: FeeType.RiskAdjustment,
                feePercentage: 0.05m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: false,
                riskIndicators: RiskIndicators.FloodZone);

            var f4 = new FeeConfiguration(
                feeName: "Risk Adj - EQ 2027",
                feeType: FeeType.RiskAdjustment,
                feePercentage: 0.07m,
                effectiveFrom: new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.EarthquakeProne);

            var f5 = new FeeConfiguration(
                feeName: "Admin Fee 2027",
                feeType: FeeType.AdminFee,
                feePercentage: 0.11m,
                effectiveFrom: new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                isActive: false,
                riskIndicators: RiskIndicators.None);

            _db.FeeConfigurations.AddRange(f1, f2, f3, f4, f5);
            _db.SaveChanges();
        }

        [Fact]
        public async Task ListFeeConfigurations_Page2_PageSize2_ShouldReturnPagedResults()
        {
            var query = new ListFeeConfigurationsQuery{ Name = null, Type = null, IsActive = null, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Equal(5, resp.TotalCount);
            Assert.Equal(3, resp.TotalPages);
            Assert.Equal(2, resp.Items.Count);
        }

        [Fact]
        public async Task ListFeeConfigurations_FilterByType_RiskAdjustment_ShouldReturnOnlyRiskAdjustment()
        {
            var query = new ListFeeConfigurationsQuery{ Name = null, Type = FeeTypeContract.RiskAdjustment, IsActive = null, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Equal(2, resp.TotalCount);
            Assert.Equal(1, resp.TotalPages);
            Assert.Equal(2, resp.Items.Count);
            Assert.All(resp.Items, x => Assert.Equal(FeeType.RiskAdjustment, x.FeeType));
        }

        [Fact]
        public async Task ListFeeConfigurations_FilterByIsActive_True_ShouldReturnOnlyActive()
        {
            var query = new ListFeeConfigurationsQuery{ Name = null, Type = null, IsActive = true, Page = 1, PageSize = 20 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Equal(3, resp.TotalCount);
            Assert.Equal(1, resp.TotalPages);
            Assert.Equal(3, resp.Items.Count);
            Assert.All(resp.Items, x => Assert.True(x.IsActive));
        }

        [Fact]
        public async Task ListFeeConfigurations_FilterByName_ShouldReturnOnlyMatching()
        {
            var query = new ListFeeConfigurationsQuery{ Name = "Admin Fee 2026", Type = null, IsActive = null, Page = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var resp = result.Value!;
            Assert.Single(resp.Items);
            Assert.Equal("Admin Fee 2026", resp.Items[0].Name);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}