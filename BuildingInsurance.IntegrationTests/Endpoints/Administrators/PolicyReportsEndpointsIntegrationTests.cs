using BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Persistence.Reporting;
using BuildingInsurance.IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Endpoints.Administrators
{
    [Trait("Category", "Integration")]
    [Collection("Integration")]
    public sealed class PolicyReportsEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PolicyReportsEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();

            SeedPolicies();
        }

        [Fact]
        public async Task PoliciesByCountry_ShouldReturnAggregatedResults()
        {
            var url =
                "/api/admin/reports/policies-by-country" +
                "?from=2026-02-01T00:00:00Z" +
                "&to=2026-02-28T23:59:59Z" +
                "&status=Active" +
                "&currency=EUR";

            var resp = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<PaginatedPolicyReportResponse>>(JsonOptions);

            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.NotNull(result.Value);

            var rows = result.Value!.Items;

            Assert.Contains(rows, r =>
                r.GroupingKey == "ROMANIA" &&
                r.CurrencyCode == "EUR" &&
                r.PolicyCount == 1 &&
                r.TotalFinalPremium == 100m);

            Assert.Contains(rows, r =>
                r.GroupingKey == "BULGARIA" &&
                r.CurrencyCode == "EUR" &&
                r.PolicyCount == 1 &&
                r.TotalFinalPremium == 300m);
        }

        [Fact]
        public async Task PoliciesByCity_ShouldFilterCorrectly()
        {
            var url =
                "/api/admin/reports/policies-by-city" +
                "?from=2026-02-01T00:00:00Z" +
                "&to=2026-02-28T23:59:59Z" +
                "&currency=EUR" +
                "&status=Active";

            var resp = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<PaginatedPolicyReportResponse>>(JsonOptions);

            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var rows = result.Value!.Items;

            Assert.DoesNotContain(rows, r => r.CurrencyCode == "RON");
            Assert.All(rows, r => Assert.Equal("EUR", r.CurrencyCode));
        }

        [Fact]
        public async Task PoliciesByBroker_ShouldGroupAndSumCorrectly()
        {
            var url =
                "/api/admin/reports/policies-by-broker" +
                "?from=2026-02-01T00:00:00Z" +
                "&to=2026-02-28T23:59:59Z" +
                "&status=Active" +
                "&currency=EUR";

            var resp = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<PaginatedPolicyReportResponse>>(JsonOptions);

            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var rows = result.Value!.Items;

            var brokerA = rows.Single(r => r.GroupingKey == "INT-001");

            Assert.Equal(2, brokerA.PolicyCount);
            Assert.Equal(400m, brokerA.TotalFinalPremium);
        }

        [Fact]
        public async Task PoliciesByCountry_WhenInvalidDateRange_ShouldReturnBadRequest()
        {
            var url = "/api/admin/reports/policies-by-country" +
                "?from=2026-03-01T00:00:00Z" +
                "&to=2026-02-01T00:00:00Z" +
                "&status=Active" +
                "&currency=EUR";

            var resp = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task PoliciesByCounty_ShouldReturnAggregatedResults()
        {
            var url =
                "/api/admin/reports/policies-by-county" +
                "?from=2026-02-01T00:00:00Z" +
                "&to=2026-02-28T23:59:59Z" +
                "&status=Active" +
                "&currency=EUR";

            var resp = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<PaginatedPolicyReportResponse>>(JsonOptions);

            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.NotNull(result.Value);

            var rows = result.Value!.Items;

            Assert.Contains(rows, r =>
                r.GroupingKey == "CLUJ" &&
                r.CurrencyCode == "EUR" &&
                r.PolicyCount == 1 &&
                r.TotalFinalPremium == 100m);

            Assert.Contains(rows, r =>
                r.GroupingKey == "SOFIA" &&
                r.CurrencyCode == "EUR" &&
                r.PolicyCount == 1 &&
                r.TotalFinalPremium == 300m);
        }

        private void SeedPolicies()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

            var client = db.Clients.FirstOrDefault();
            if (client is null)
            {
                client = new Domain.Entities.Clients.Client(
                    id: Guid.NewGuid(),
                    type: ClientType.Individual,
                    fullName: "Reports Client",
                    contactInfo: new Domain.ValueObjects.ContactInfo("reports.client@test.com", "0700000000", null),
                    personalIdentificationNumber: "1111111111111",
                    companyRegistrationNumber: null);

                db.Clients.Add(client);
                db.SaveChanges();
            }

            var bgCountry = new Domain.Entities.Geography.Country(Guid.NewGuid(), "BULGARIA");
            db.Countries.Add(bgCountry);
            db.SaveChanges();

            var sofiaCounty = new Domain.Entities.Geography.County(Guid.NewGuid(), "SOFIA", bgCountry.Id);
            db.Counties.Add(sofiaCounty);
            db.SaveChanges();

            var sofiaCity = new Domain.Entities.Geography.City(Guid.NewGuid(), "SOFIA", sofiaCounty.Id);
            db.Cities.Add(sofiaCity);
            db.SaveChanges();

            var clujCity = db.Cities.FirstOrDefault(c => c.Name == "CLUJ-NAPOCA");

            var brokerA = db.Brokers.FirstOrDefault(b => b.BrokerCode == "INT-001");

            var eur = db.Currencies.FirstOrDefault(c => c.Code == "EUR");
            if (eur is null)
                throw new InvalidOperationException("Seed error: EUR currency not found.");

            void AddActivePolicy(Guid cityId, decimal finalPremium, DateTime startUtc)
            {
                var building = new Domain.Entities.Buildings.Building(
                    id: Guid.NewGuid(),
                    clientId: client!.Id,
                    address: new Domain.ValueObjects.Address("Main", "1"),
                    cityId: cityId,
                    constructionYear: 2000,
                    type: BuildingType.Residential,
                    numberOfFloors: 1,
                    surfaceArea: 100m,
                    insuredValue: 100_000m,
                    riskIndicators: RiskIndicators.None);

                db.Buildings.Add(building);

                var policy = Domain.Entities.Policies.Policy.CreateDraft(
                    clientId: client.Id,
                    buildingId: building.Id,
                    brokerId: brokerA!.Id,
                    currencyId: eur!.Id,
                    startDate: startUtc,
                    endDate: startUtc.AddYears(1),
                    basePremium: 50m);

                policy.SetPricing(finalPremium, Enumerable.Empty<Domain.Entities.Policies.PolicyAppliedFee>(), Enumerable.Empty<Domain.Entities.Policies.PolicyAppliedRiskFactor>());
                policy.SetFinalPremiumInBaseCurrency(finalPremium);
                policy.Activate(startUtc);

                db.Policies.Add(policy);
            }

            var start1 = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc);
            AddActivePolicy(clujCity!.Id, 100m, start1);
            AddActivePolicy(sofiaCity.Id, 300m, start1.AddDays(5));
            
            db.PolicyReportFacts.AddRange(
                new PolicyReportFact
                {
                    PolicyId = Guid.NewGuid(),
                    StartDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                    PolicyStatus = PolicyStatus.Active,
                    CurrencyId = eur.Id,
                    FinalPremium = 100m,
                    FinalPremiumInBaseCurrency = 100m,
                    BrokerId = brokerA!.Id,
                    BrokerCode = brokerA.BrokerCode,
                    CityId = clujCity!.Id,
                    BuildingType = BuildingType.Residential,
                    SourceLastUpdatedUtc = DateTime.UtcNow
                },
                new PolicyReportFact
                {
                    PolicyId = Guid.NewGuid(),
                    StartDate = new DateTime(2026, 02, 15, 0, 0, 0, DateTimeKind.Utc),
                    PolicyStatus = PolicyStatus.Active,
                    CurrencyId = eur.Id,
                    FinalPremium = 300m,
                    FinalPremiumInBaseCurrency = 300m,
                    BrokerId = brokerA.Id,
                    BrokerCode = brokerA.BrokerCode,
                    CityId = sofiaCity.Id,
                    BuildingType = BuildingType.Residential,
                    SourceLastUpdatedUtc = DateTime.UtcNow
                }
            );

            db.SaveChanges();
        }

        public void Dispose() => _client.Dispose();
        private sealed class PolicyReportRow
        {
            public string GroupingKey { get; set; } = "";
            public string CurrencyCode { get; set; } = "";
            public int PolicyCount { get; set; }
            public decimal TotalFinalPremium { get; set; }
            public decimal TotalFinalPremiumInBaseCurrency { get; set; }
        }

        private sealed class PaginatedPolicyReportResponse
        {
            public List<PolicyReportRow> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class ApiResult<T>
        {
            public T? Value { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsFailure { get; set; }
            public string Error { get; set; } = "";
            public string ErrorType { get; set; } = "";
        }
    }
}