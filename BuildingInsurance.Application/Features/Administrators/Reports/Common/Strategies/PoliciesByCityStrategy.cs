using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies
{
    public sealed class PoliciesByCityStrategy : IPolicyReportStrategy
    {
        private readonly IPolicyReportsRepository _policyReportsRepository;

        public PoliciesByCityStrategy(IPolicyReportsRepository policyReportsRepository)
        {
            _policyReportsRepository = policyReportsRepository;
        }

        public bool CanHandle(ReportDimension dimension) => dimension == ReportDimension.City;

        public Task<List<PolicyReportRowDto>> GenerateReportAsync(DateTime from, DateTime to, PolicyStatus status, string currencyCode, BuildingType? buildingType, CancellationToken ct)
        {
            return _policyReportsRepository.GetPoliciesGroupedAsync(ReportDimension.City, from, to, status, currencyCode, buildingType, ct);
        }
    }
}