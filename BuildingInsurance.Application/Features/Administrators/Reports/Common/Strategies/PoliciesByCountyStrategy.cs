using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies
{
    public sealed class PoliciesByCountyStrategy : IPolicyReportStrategy
    {
        private readonly IPolicyReportsRepository _policyReportsRepository;

        public PoliciesByCountyStrategy(IPolicyReportsRepository policyReportsRepository)
        {
            _policyReportsRepository = policyReportsRepository;
        }
        public bool CanHandle(ReportDimension dimension) => dimension == ReportDimension.County;

        public Task<List<PolicyReportRowDto>> GenerateReportAsync(DateTime from, DateTime to, PolicyStatus status, string currencyCode, BuildingType? buildingType, CancellationToken ct)
        {
            return _policyReportsRepository.GetPoliciesGroupedAsync(ReportDimension.County, from, to, status, currencyCode, buildingType, ct);
        }
    }
}
