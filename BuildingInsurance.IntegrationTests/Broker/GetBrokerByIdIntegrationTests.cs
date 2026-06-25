using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Queries.GetBrokerById;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
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

namespace BuildingInsurance.IntegrationTests.Broker
{
    public sealed class GetBrokerByIdIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly GetBrokerByIdHandler _handler;

        private readonly Guid _brokerId = Guid.NewGuid();

        public GetBrokerByIdIntegrationTests()
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
            var riskFactorRepo = new RiskFactorConfigurationRepository(_db);
            var feeRepo = new FeeConfigurationRepository(_db);

            _uow = new UnitOfWork(_db, clientRepo, buildingRepo, cityRepo, countyRepo, countryRepo, policyRepo, brokerRepo, currencyRepo, riskFactorRepo, feeRepo);
            _handler = new GetBrokerByIdHandler(_uow);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var broker = new Domain.Entities.Management.Broker(
                brokerCode: "BRK-GET",
                name: "Broker Get",
                contactInfo: new ContactInfo("get@broker.com", "0700000101"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.20m);

            typeof(Domain.Entities.Management.Broker)
                .GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(broker, _brokerId);

            _db.Brokers.Add(broker);
            _db.SaveChanges();
        }

        [Fact]
        public async Task GetBrokerById_WhenNotFound_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();
            var query = new GetBrokerByIdQuery(missingId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Broker with ID {missingId} not found.", result.Error);
        }

        [Fact]
        public async Task GetBrokerById_WhenExists_ShouldReturnDto()
        {
            var query = new GetBrokerByIdQuery(_brokerId);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var dto = result.Value!;
            Assert.Equal(_brokerId, dto.Id);
            Assert.Equal("BRK-GET", dto.BrokerCode);
            Assert.Equal("Broker Get", dto.FullName);
            Assert.Equal("get@broker.com", dto.Email);
            Assert.Equal("0700000101", dto.Phone);
            Assert.Equal(BrokerStatus.Active, dto.BrokerStatus);
            Assert.Equal(0.20m, dto.CommissionPercentage);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}