using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.GetFeeConfigurationById;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.BuildingsRepository;
using BuildingInsurance.Infrastructure.Repositories.ClientsRepository;
using BuildingInsurance.Infrastructure.Repositories.GeographyRepository;
using BuildingInsurance.Infrastructure.Repositories.ManagementRepository;
using BuildingInsurance.Infrastructure.Repositories.MetadataRepository;
using BuildingInsurance.Infrastructure.Repositories.PolicyRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.FeeConfigurations
{
    public sealed class GetFeeConfigurationByIdIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly GetFeeConfigurationByIdHandler _handler;

        private readonly Guid _adminFeeId = Guid.NewGuid();
        private readonly Guid _riskAdjustmentFeeId = Guid.NewGuid();

        public GetFeeConfigurationByIdIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            var clientRepo = new ClientRepository(_db);
            var buildingRepo = new BuildingRepository(_db);
            var cityRepo = new CityRepository(_db);
            var countyRepo = new CountyRepository(_db);
            var countryRepo = new CountryRepository(_db);
            var policyRepo = new PolicyRepository(_db);
            var brokerRepo = new BrokerRepository(_db);
            var currencyRepo = new CurrencyRepository(_db);
            var riskFactorConfigRepo = new RiskFactorConfigurationRepository(_db);
            var feeConfigRepo = new FeeConfigurationRepository(_db);

            _uow = new UnitOfWork(
                _db,
                clientRepo,
                buildingRepo,
                cityRepo,
                countyRepo,
                countryRepo,
                policyRepo,
                brokerRepo,
                currencyRepo,
                riskFactorConfigRepo,
                feeConfigRepo
            );

            _handler = new GetFeeConfigurationByIdHandler(_uow);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var adminFee = new Domain.Entities.Metadata.FeeConfiguration(
                feeName: "Admin Fee 2026",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: true,
                riskIndicators: RiskIndicators.None
            );

            var riskAdjustment = new Domain.Entities.Metadata.FeeConfiguration(
                feeName: "Risk Adj - Flood",
                feeType: FeeType.RiskAdjustment,
                feePercentage: 0.05m,
                effectiveFrom: new DateTime(2027, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo: new DateTime(2027, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive: false,
                riskIndicators: RiskIndicators.FloodZone
            );

            typeof(Domain.Entities.Metadata.FeeConfiguration)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(adminFee, _adminFeeId);

            typeof(Domain.Entities.Metadata.FeeConfiguration)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(riskAdjustment, _riskAdjustmentFeeId);

            _db.FeeConfigurations.AddRange(adminFee, riskAdjustment);
            _db.SaveChanges();
        }

        [Fact]
        public async Task GetFeeConfigurationById_WhenNotFound_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();
            var query = new GetFeeConfigurationByIdQuery(missingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Fee configuration not found.", result.Error);
        }

        [Fact]
        public async Task GetFeeConfigurationById_WhenExists_AdminFee_ShouldReturnDto()
        {
            var query = new GetFeeConfigurationByIdQuery(_adminFeeId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var dto = result.Value!;
            Assert.Equal(_adminFeeId, dto.Id);
            Assert.Equal("Admin Fee 2026", dto.Name);
            Assert.Equal(FeeType.AdminFee, dto.FeeType);
            Assert.Equal(0.10m, dto.FeePercentage);
            Assert.Equal(new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc), dto.EffectiveFrom);
            Assert.Equal(new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc), dto.EffectiveTo);
            Assert.True(dto.IsActive);
            Assert.Equal(RiskIndicators.None, dto.RiskIndicators);

            var persisted = await _db.FeeConfigurations.AsNoTracking().FirstAsync(f => f.Id == _adminFeeId);
            Assert.Equal("Admin Fee 2026", persisted.Name);
            Assert.Equal(FeeType.AdminFee, persisted.FeeType);
        }

        [Fact]
        public async Task GetFeeConfigurationById_WhenExists_RiskAdjustmentFee_ShouldReturnDto()
        {
            var query = new GetFeeConfigurationByIdQuery(_riskAdjustmentFeeId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var dto = result.Value!;
            Assert.Equal(_riskAdjustmentFeeId, dto.Id);
            Assert.Equal("Risk Adj - Flood", dto.Name);
            Assert.Equal(FeeType.RiskAdjustment, dto.FeeType);
            Assert.Equal(0.05m, dto.FeePercentage);
            Assert.False(dto.IsActive);
            Assert.Equal(RiskIndicators.FloodZone, dto.RiskIndicators);

            var persisted = await _db.FeeConfigurations.AsNoTracking().FirstAsync(f => f.Id == _riskAdjustmentFeeId);
            Assert.Equal(FeeType.RiskAdjustment, persisted.FeeType);
            Assert.Equal(RiskIndicators.FloodZone, persisted.RiskIndicators);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}