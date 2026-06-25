using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies
{
    public sealed class PoliciesByBrokerStrategy : IPolicyReportStrategy
    {
        private readonly IPolicyReportsRepository _policyReportsRepository;

        public PoliciesByBrokerStrategy(IPolicyReportsRepository policyReportsRepository)
        {
            _policyReportsRepository = policyReportsRepository;
        }

        public bool CanHandle(ReportDimension dimension) => dimension == ReportDimension.Broker;

        public Task<List<PolicyReportRowDto>> GenerateReportAsync(DateTime from, DateTime to, PolicyStatus status, string currencyCode, BuildingType? buildingType, CancellationToken ct)
        {
            return _policyReportsRepository.GetPoliciesGroupedAsync(ReportDimension.Broker, from, to, status, currencyCode, buildingType, ct);
        }
    }
}