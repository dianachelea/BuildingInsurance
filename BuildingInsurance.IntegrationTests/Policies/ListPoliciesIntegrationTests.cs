using BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Repositories.PolicyRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Policies
{
    public sealed class ListPoliciesIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ListPoliciesHandler _handler;

        private Guid _client1;
        private Guid _client2;
        private Guid _broker1;
        private Guid _broker2;
        private Guid _currencyId;
        private Guid _building1;
        private Guid _building2;

        public ListPoliciesIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            _handler = new ListPoliciesHandler(new PolicyRepository(_db));

            Seed();
        }

        private void Seed()
        {
            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            var city = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);

            _db.Countries.Add(country);
            _db.Counties.Add(county);
            _db.Cities.Add(city);

            var c1 = new Domain.Entities.Clients.Client(Guid.NewGuid(), ClientType.Individual, "Client 1", new ContactInfo("c1@x.com", "0700", null), "1111111111111", null);
            var c2 = new Domain.Entities.Clients.Client(Guid.NewGuid(), ClientType.Individual, "Client 2", new ContactInfo("c2@x.com", "0701", null), "2222222222222", null);
            _db.Clients.AddRange(c1, c2);
            _client1 = c1.Id;
            _client2 = c2.Id;

            var b1 = new Domain.Entities.Management.Broker("BRK1", "Broker 1", new ContactInfo("b1@x.com", "0702"), BrokerStatus.Active, 0.10m);
            var b2 = new Domain.Entities.Management.Broker("BRK2", "Broker 2", new ContactInfo("b2@x.com", "0703"), BrokerStatus.Active, 0.10m);
            _db.Brokers.AddRange(b1, b2);
            _broker1 = b1.Id;
            _broker2 = b2.Id;

            var cur = new Domain.Entities.Metadata.Currency("EUR", "Euro", 1.0m, true);
            _db.Currencies.Add(cur);
            _currencyId = cur.Id;

            var building1 = new Domain.Entities.Buildings.Building(Guid.NewGuid(), c1.Id, new Address("Street", "1"), city.Id, 2000, BuildingType.Residential, 1, 50m, 50_000m, RiskIndicators.None);
            var building2 = new Domain.Entities.Buildings.Building(Guid.NewGuid(), c2.Id, new Address("Street", "2"), city.Id, 2001, BuildingType.Residential, 1, 60m, 60_000m, RiskIndicators.None);
            _db.Buildings.AddRange(building1, building2);
            _building1 = building1.Id;
            _building2 = building2.Id;

            var p1 = Policy.CreateDraft(c1.Id, building1.Id, b1.Id, cur.Id, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), 100m);
            p1.SetPricing(120m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            p1.SetFinalPremiumInBaseCurrency(120m);
            p1.Activate(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var p2 = Policy.CreateDraft(c1.Id, building1.Id, b1.Id, cur.Id, new DateTime(2027, 2, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2028, 2, 1, 0, 0, 0, DateTimeKind.Utc), 100m);
            p2.SetPricing(110m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());

            var p3 = Policy.CreateDraft(c2.Id, building2.Id, b1.Id, cur.Id, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2027, 5, 1, 0, 0, 0, DateTimeKind.Utc), 200m);
            p3.SetPricing(220m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            p3.SetFinalPremiumInBaseCurrency(220m);
            p3.Activate(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));

            var p4 = Policy.CreateDraft(c2.Id, building2.Id, b2.Id, cur.Id, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc), 200m);
            p4.SetPricing(230m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());
            p4.SetFinalPremiumInBaseCurrency(230m);
            p4.Activate(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            p4.Cancel("Customer request", new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));

            var p5 = Policy.CreateDraft(c1.Id, building1.Id, b2.Id, cur.Id, new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2027, 8, 1, 0, 0, 0, DateTimeKind.Utc), 150m);
            p5.SetPricing(160m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());

            var p6 = Policy.CreateDraft(c2.Id, building2.Id, b2.Id, cur.Id, new DateTime(2027, 9, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2028, 9, 1, 0, 0, 0, DateTimeKind.Utc), 150m);
            p6.SetPricing(170m, Array.Empty<PolicyAppliedFee>(), Array.Empty<PolicyAppliedRiskFactor>());

            _db.Policies.AddRange(p1, p2, p3, p4, p5, p6);
            _db.SaveChanges();
        }

        [Fact]
        public async Task ListPolicies_Page2_PageSize2_ShouldReturnPagedCounts()
        {
            var query = new ListPoliciesQuery{ ClientId = null, BrokerId = null, Status = null, Page = 2, PageSize = 2 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(6, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);
        }

        [Fact]
        public async Task ListPolicies_FilterByBroker_ShouldReturnOnlyThatBroker()
        {
            var query = new ListPoliciesQuery{ ClientId = null, BrokerId = _broker1, Status = null, Page = 1, PageSize = 20 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.All(result.Value!.Items, x => Assert.Equal(_broker1, x.BrokerId));
        }

        [Fact]
        public async Task ListPolicies_FilterByClient_ShouldReturnOnlyThatClient()
        {
            var query = new ListPoliciesQuery{ ClientId = _client1, BrokerId = null, Status = null, Page = 1, PageSize = 20 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.All(result.Value!.Items, x => Assert.Equal(_client1, x.ClientId));
        }

        [Fact]
        public async Task ListPolicies_FilterByStatus_Active_ShouldReturnOnlyActive()
        {
            var query = new ListPoliciesQuery { ClientId = null, BrokerId = null, Status = PolicyStatusContract.Active, Page = 1, PageSize = 20};

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.All(result.Value!.Items, x => Assert.Equal(PolicyStatus.Active, x.PolicyStatus));
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    }
}