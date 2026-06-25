using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Extensions;
using BuildingInsurance.Application.Features.Common.Mapping;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Services
{
    public sealed class ReportJobProcessor : IReportJobProcessor
    {
        private readonly IPolicyReportStrategySelector _selector;

        public ReportJobProcessor(IPolicyReportStrategySelector selector)
        {
            _selector = selector;
        }

        public async Task<List<PolicyReportRowDto>> GenerateAsync(ReportDimension dimension, PolicyReportFilters filters, CancellationToken ct)
        {
            var strategy = _selector.Select(dimension);

            var rows = await strategy.GenerateReportAsync(
                from: filters.From.ToUtc(),
                to: filters.To.ToUtc(),
                status: filters.Status.HasValue ? filters.Status.Value.MapToDomainPolicyStatus() : default,
                currencyCode: filters.CurrencyCode!,
                buildingType: filters.BuildingType.MapToDomainBuildingTypeOptional(),
                ct: ct);

            return rows ?? new List<PolicyReportRowDto>();
        }
    }
}