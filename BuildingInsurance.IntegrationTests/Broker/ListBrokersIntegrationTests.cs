using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers;
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
    public sealed class ListBrokersIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly ListBrokersHandler _handler;

        public ListBrokersIntegrationTests()
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
            _handler = new ListBrokersHandler(_uow);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _db.Brokers.AddRange(
                new Domain.Entities.Management.Broker("BRK-A", "Alice Broker", new ContactInfo("alice@b.com", "0700000001"), BrokerStatus.Active, 0.10m),
                new Domain.Entities.Management.Broker("BRK-B", "Bob Broker", new ContactInfo("bob@b.com", "0700000002"), BrokerStatus.Inactive, 0.12m),
                new Domain.Entities.Management.Broker("BRK-C", "Charlie Broker", new ContactInfo("charlie@b.com", "0700000003"), BrokerStatus.Active, null),
                new Domain.Entities.Management.Broker("BRK-D", "David Broker", new ContactInfo("david@b.com", "0700000004"), BrokerStatus.Inactive, 0.18m),
                new Domain.Entities.Management.Broker("BRK-E", "Eve Broker", new ContactInfo("eve@b.com", "0700000005"), BrokerStatus.Active, 0.15m)
            );

            _db.SaveChanges();
        }

        [Fact]
        public async Task ListBrokers_Page2_PageSize2_ShouldReturnPagedResults()
        {
            var query = new ListBrokersQuery{Name = null,IsActive = null,Page = 2,PageSize = 2};

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var expectedTotal = await _db.Brokers.AsNoTracking().CountAsync();
            var expectedPages = expectedTotal == 0 ? 0 : (int)Math.Ceiling(expectedTotal / 2d);

            Assert.Equal(expectedTotal, result.Value!.TotalCount);
            Assert.Equal(expectedPages, result.Value.TotalPages);
            Assert.True(result.Value.Items.Count <= 2);
        }

        [Fact]
        public async Task ListBrokers_FilterByIsActive_True_ShouldReturnOnlyActive()
        {
            var query = new ListBrokersQuery { Name = null, IsActive = true, Page = 1, PageSize = 20};

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.All(result.Value!.Items, x => Assert.Equal(BrokerStatus.Active, x.BrokerStatus));
        }

        [Fact]
        public async Task ListBrokers_FilterByName_ShouldReturnMatching()
        {
            var query = new ListBrokersQuery{ Name = "Alice", IsActive = null, Page = 1, PageSize = 20 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.True(result.Value!.Items.Count >= 1);
            Assert.All(result.Value.Items, x => Assert.Contains("Alice", x.FullName));
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}