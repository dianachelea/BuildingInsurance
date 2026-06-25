using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Repositories.ReportsRepository
{
    public sealed class PolicyReportsRepository : IPolicyReportsRepository
    {
        private readonly BuildingInsuranceDbContext _buildingInsuranceDbContext;
        private readonly IGeographyCachingService _geographyCachingService;
        private readonly ICurrencyCachingService _currencyCachingService;

        public PolicyReportsRepository(BuildingInsuranceDbContext buildingInsuranceDbContext, IGeographyCachingService geographyCachingService, ICurrencyCachingService currencyCachingService)
        {
            _buildingInsuranceDbContext = buildingInsuranceDbContext;
            _geographyCachingService = geographyCachingService;
            _currencyCachingService = currencyCachingService;
        }

        public async Task<List<PolicyReportRowDto>> GetPoliciesGroupedAsync(ReportDimension dimension, DateTime from, DateTime to, PolicyStatus status, string currencyCode, BuildingType? buildingType, CancellationToken ct)
        {
            var normalizedCurrencyCode = (currencyCode ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedCurrencyCode))
                return new List<PolicyReportRowDto>();

            if (!_currencyCachingService.TryGetId(normalizedCurrencyCode, out var currencyId))
                return new List<PolicyReportRowDto>();

            if (!_currencyCachingService.TryGetCode(currencyId, out var currencyCodeStr))
                return new List<PolicyReportRowDto>();

            if (dimension == ReportDimension.Broker)
            {
                return await GetByBrokerAsync(from, to, status, currencyId, currencyCodeStr, buildingType, ct);
            }
            else if (dimension == ReportDimension.City || dimension == ReportDimension.County || dimension == ReportDimension.Country)
            {
                var geoParams = new ReportFilters
                {
                    Dimension = dimension,
                    From = from,
                    To = to,
                    Status = status,
                    CurrencyId = currencyId,
                    CurrencyCodeStr = currencyCodeStr,
                    BuildingType = buildingType
                };
                return await GetByGeoAsync(geoParams, ct);
            }
            else
                return new List<PolicyReportRowDto>();
        }

        private async Task<List<PolicyReportRowDto>> GetByGeoAsync(ReportFilters filters, CancellationToken cancellationToken)
        {
            var policyReports = _buildingInsuranceDbContext.PolicyReportFacts
                .AsNoTracking()
                .Where(f => f.StartDate >= filters.From && f.StartDate <= filters.To)
                .Where(f => f.PolicyStatus == filters.Status)
                .Where(f => f.CurrencyId == filters.CurrencyId);

            if (filters.BuildingType.HasValue)
                policyReports = policyReports.Where(f => f.BuildingType == filters.BuildingType.Value);
            
            if (!await policyReports.AnyAsync(cancellationToken))
                return new List<PolicyReportRowDto>();

            var byCity = await policyReports
                .GroupBy(f => f.CityId)
                .Select(g => new CityAggRow
                {
                    CityId = g.Key,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremiumInBaseCurrency)
                })
                .ToListAsync(cancellationToken);

            if (byCity.Count == 0)
                return new List<PolicyReportRowDto>();

            var dict = AggregateGeoRows(byCity, filters.Dimension);

            if (dict.Count == 0)
                return new List<PolicyReportRowDto>();

            return dict
                .Select(d => new PolicyReportRowDto(
                    GroupingKey: d.Key,
                    CurrencyCode: filters.CurrencyCodeStr,
                    PolicyCount: d.Value.Count,
                    TotalFinalPremium: d.Value.Sum,
                    TotalFinalPremiumInBaseCurrency: d.Value.SumBase))
                .OrderByDescending(x => x.TotalFinalPremium)
                .ToList();
        }

        private Dictionary<string, (int Count, decimal Sum, decimal SumBase)> AggregateGeoRows(List<CityAggRow> byCity, ReportDimension dimension)
        {
            var dict = new Dictionary<string, (int Count, decimal Sum, decimal SumBase)>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in byCity)
            {
                if (!_geographyCachingService.TryGet(row.CityId, out var city, out var county, out var country))
                    continue;

                var groupingKey = GetGroupingKey(dimension, city, county, country);
                if (string.IsNullOrWhiteSpace(groupingKey))
                    continue;

                var agg = dict.GetValueOrDefault(groupingKey);
                dict[groupingKey] = (
                    Count: agg.Count + row.PolicyCount,
                    Sum: agg.Sum + row.TotalFinalPremium,
                    SumBase: agg.SumBase + row.TotalFinalPremiumInBaseCurrency
                );
            }

            return dict;
        }

        private static string GetGroupingKey(ReportDimension dimension, string city, string county, string country)
        {
            if (dimension == ReportDimension.City)
                return city;

            if (dimension == ReportDimension.County)
                return county;

            if (dimension == ReportDimension.Country)
                return country;

            throw new InvalidOperationException($"Unsupported geo dimension {dimension}");
        }
        
        private async Task<List<PolicyReportRowDto>> GetByBrokerAsync(DateTime from, DateTime to, PolicyStatus status, Guid currencyId, string currencyCodeStr, BuildingType? buildingType, CancellationToken ct)
        {
            var grouped = await _buildingInsuranceDbContext.PolicyReportFacts
                .AsNoTracking()
                .Where(f => f.StartDate >= from && f.StartDate <= to)
                .Where(f => f.PolicyStatus == status)
                .Where(f => f.CurrencyId == currencyId)
                .Where(f => !buildingType.HasValue || f.BuildingType == buildingType.Value)
                .GroupBy(f => f.BrokerCode)
                .Select(g => new
                {
                    BrokerCode = g.Key,
                    PolicyCount = g.Count(),
                    TotalFinalPremium = g.Sum(x => x.FinalPremium),
                    TotalFinalPremiumInBaseCurrency = g.Sum(x => x.FinalPremiumInBaseCurrency)
                })
                .ToListAsync(ct);

            return grouped
                .OrderByDescending(x => x.TotalFinalPremium)
                .Select(x => new PolicyReportRowDto(
                    x.BrokerCode,
                    currencyCodeStr,
                    x.PolicyCount,
                    x.TotalFinalPremium,
                    x.TotalFinalPremiumInBaseCurrency))
                .ToList();
        }

        private sealed class ReportFilters
        {
            public ReportDimension Dimension { get; init; }
            public DateTime From { get; init; }
            public DateTime To { get; init; }
            public PolicyStatus Status { get; init; }
            public Guid CurrencyId { get; init; }
            public string CurrencyCodeStr { get; init; } = string.Empty;
            public BuildingType? BuildingType { get; init; }
        }

        private sealed class CityAggRow
        {
            public Guid CityId { get; init; }
            public int PolicyCount { get; init; }
            public decimal TotalFinalPremium { get; init; }
            public decimal TotalFinalPremiumInBaseCurrency { get; init; }
        }
    }
}