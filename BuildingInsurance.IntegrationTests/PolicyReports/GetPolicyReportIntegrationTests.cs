using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies;
using BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Persistence.Reporting;
using BuildingInsurance.Infrastructure.Repositories.ReportsRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BuildingInsurance.IntegrationTests.PolicyReports
{
    public sealed class GetPolicyReportIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly BuildingInsuranceDbContext _db;

        private readonly FakeGeographyCachingService _geography;
        private readonly FakeCurrencyCachingService _currency;

        private readonly IPolicyReportsRepository _policyReportsRepository;

        private readonly GetPolicyReportHandler _handler;

        private Guid _cityClujId;
        private Guid _citySofiaId;

        private Guid _brokerAId;
        private Guid _brokerBId;

        private Guid _eurId;
        private Guid _ronId;

        public GetPolicyReportIntegrationTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new BuildingInsuranceDbContext(options);
            _db.Database.EnsureCreated();

            _geography = new FakeGeographyCachingService();
            _currency = new FakeCurrencyCachingService();

            SeedReportingFacts();

            _policyReportsRepository = new PolicyReportsRepository(_db, _geography, _currency);

            var strategies = new IPolicyReportStrategy[]
            {
                new PoliciesByCountryStrategy(_policyReportsRepository),
                new PoliciesByCountyStrategy(_policyReportsRepository),
                new PoliciesByCityStrategy(_policyReportsRepository),
                new PoliciesByBrokerStrategy(_policyReportsRepository),
            };

            var selector = new PolicyReportStrategySelector(strategies);

            _handler = new GetPolicyReportHandler(selector, NullLogger<GetPolicyReportHandler>.Instance);
        }

        [Fact]
        public async Task GetPolicyReport_Country_WhenInRange_ShouldGroupByCountryAndCurrency_AndSumCorrectly()
        {
            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Country,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 23, 59, 59, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: null));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var response = result.Value!;
            var rows = response.Items;

            Assert.Contains(rows, r =>
                r.GroupingKey == "Romania" &&
                r.CurrencyCode == "EUR" &&
                r.PolicyCount == 1 &&
                r.TotalFinalPremium == 100m);

            Assert.Contains(rows, r =>
                r.GroupingKey == "Bulgaria" &&
                r.CurrencyCode == "EUR" &&
                r.PolicyCount == 1 &&
                r.TotalFinalPremium == 300m);

            var roEur = rows.Single(r => r.GroupingKey == "Romania" && r.CurrencyCode == "EUR");
            Assert.Equal(100m, roEur.TotalFinalPremium);
        }

        [Fact]
        public async Task GetPolicyReport_Broker_WhenFilteredByStatusCurrencyAndBuildingType_ShouldReturnOnlyMatching()
        {
            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.Broker,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 23, 59, 59, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: BuildingTypeContract.Residential));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var response = result.Value!;
            var rows = response.Items;
            Assert.Single(rows);

            var row = rows[0];
            Assert.Equal("BRK-A", row.GroupingKey);
            Assert.Equal("EUR", row.CurrencyCode);
            Assert.Equal(2, row.PolicyCount);
            Assert.Equal(400m, row.TotalFinalPremium);
        }

        [Fact]
        public async Task GetPolicyReport_City_WhenGeographyMissing_ShouldReturnEmpty()
        {
            _geography.ShouldSucceed = false;

            var query = new GetPolicyReportQuery(
                Dimension: ReportDimension.City,
                Filters: new PolicyReportFilters(
                    From: new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    To: new DateTime(2026, 02, 28, 23, 59, 59, DateTimeKind.Utc),
                    Status: PolicyStatusContract.Active,
                    CurrencyCode: "EUR",
                    BuildingType: BuildingTypeContract.Residential));

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);
        }

        private void SeedReportingFacts()
        {
            _cityClujId = Guid.NewGuid();
            _citySofiaId = Guid.NewGuid();

            _brokerAId = Guid.NewGuid();
            _brokerBId = Guid.NewGuid();

            _eurId = Guid.NewGuid();
            _ronId = Guid.NewGuid();

            _currency.Add(_eurId, "EUR");
            _currency.Add(_ronId, "RON");

            _geography.ShouldSucceed = true;
            _geography.Add(_cityClujId, "Cluj-Napoca", "Cluj", "Romania");
            _geography.Add(_citySofiaId, "Sofia", "Sofia", "Bulgaria");

            _db.PolicyReportFacts.AddRange(
                new PolicyReportFact
                {
                    PolicyId = Guid.NewGuid(),
                    StartDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                    PolicyStatus = PolicyStatus.Active,
                    CurrencyId = _eurId,
                    FinalPremium = 100m,
                    FinalPremiumInBaseCurrency = 100m,
                    BrokerId = _brokerAId,
                    BrokerCode = "BRK-A",
                    CityId = _cityClujId,
                    BuildingType = BuildingType.Residential,
                    SourceLastUpdatedUtc = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new PolicyReportFact
                {
                    PolicyId = Guid.NewGuid(),
                    StartDate = new DateTime(2026, 02, 20, 0, 0, 0, DateTimeKind.Utc),
                    PolicyStatus = PolicyStatus.Active,
                    CurrencyId = _eurId,
                    FinalPremium = 300m,
                    FinalPremiumInBaseCurrency = 300m,
                    BrokerId = _brokerAId,
                    BrokerCode = "BRK-A",
                    CityId = _citySofiaId,
                    BuildingType = BuildingType.Residential,
                    SourceLastUpdatedUtc = new DateTime(2026, 02, 20, 0, 0, 0, DateTimeKind.Utc)
                },
                new PolicyReportFact
                {
                    PolicyId = Guid.NewGuid(),
                    StartDate = new DateTime(2026, 02, 22, 0, 0, 0, DateTimeKind.Utc),
                    PolicyStatus = PolicyStatus.Cancelled,
                    CurrencyId = _eurId,
                    FinalPremium = 111m,
                    FinalPremiumInBaseCurrency = 111m,
                    BrokerId = _brokerAId,
                    BrokerCode = "BRK-A",
                    CityId = _cityClujId,
                    BuildingType = BuildingType.Residential,
                    SourceLastUpdatedUtc = new DateTime(2026, 02, 22, 0, 0, 0, DateTimeKind.Utc)
                },
                new PolicyReportFact
                {
                    PolicyId = Guid.NewGuid(),
                    StartDate = new DateTime(2026, 01, 15, 0, 0, 0, DateTimeKind.Utc),
                    PolicyStatus = PolicyStatus.Active,
                    CurrencyId = _eurId,
                    FinalPremium = 999m,
                    FinalPremiumInBaseCurrency = 999m,
                    BrokerId = _brokerAId,
                    BrokerCode = "BRK-A",
                    CityId = _cityClujId,
                    BuildingType = BuildingType.Residential,
                    SourceLastUpdatedUtc = new DateTime(2026, 01, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new PolicyReportFact
                {
                    PolicyId = Guid.NewGuid(),
                    StartDate = new DateTime(2026, 02, 12, 0, 0, 0, DateTimeKind.Utc),
                    PolicyStatus = PolicyStatus.Active,
                    CurrencyId = _ronId,
                    FinalPremium = 200m,
                    FinalPremiumInBaseCurrency = 200m,
                    BrokerId = _brokerBId,
                    BrokerCode = "BRK-B",
                    CityId = _cityClujId,
                    BuildingType = BuildingType.Industrial,
                    SourceLastUpdatedUtc = new DateTime(2026, 02, 12, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        private sealed class FakeGeographyCachingService : IGeographyCachingService
        {
            public bool ShouldSucceed { get; set; } = true;

            private readonly Dictionary<Guid, (string City, string County, string Country)> _data =
                new Dictionary<Guid, (string City, string County, string Country)>();

            public Task LoadAsync(CancellationToken ct) => Task.CompletedTask;

            public void Add(Guid cityId, string city, string county, string country)
            {
                _data[cityId] = (city, county, country);
            }

            public bool TryGet(Guid cityId, out string city, out string county, out string country)
            {
                if (!ShouldSucceed)
                {
                    city = county = country = string.Empty;
                    return false;
                }

                if (_data.TryGetValue(cityId, out var geo))
                {
                    city = geo.City;
                    county = geo.County;
                    country = geo.Country;
                    return true;
                }

                city = county = country = string.Empty;
                return false;
            }
        }

        private sealed class FakeCurrencyCachingService : ICurrencyCachingService
        {
            private readonly Dictionary<Guid, string> _codeById = new Dictionary<Guid, string>();
            private readonly Dictionary<string, Guid> _idByCode = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

            public Task LoadAsync(CancellationToken ct) => Task.CompletedTask;

            public void Add(Guid currencyId, string code)
            {
                _codeById[currencyId] = code;
                _idByCode[code] = currencyId;
            }

            public bool TryGetCode(Guid currencyId, out string code)
            {
                if (_codeById.TryGetValue(currencyId, out var v))
                {
                    code = v;
                    return true;
                }

                code = string.Empty;
                return false;
            }

            public bool TryGetId(string code, out Guid id)
            {
                return _idByCode.TryGetValue(code, out id);
            }
        }
    }
}