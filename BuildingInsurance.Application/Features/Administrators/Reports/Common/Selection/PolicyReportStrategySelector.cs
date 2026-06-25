using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Strategies;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection
{
    public sealed class PolicyReportStrategySelector : IPolicyReportStrategySelector
    {
        private readonly IEnumerable<IPolicyReportStrategy> _strategies;

        public PolicyReportStrategySelector(IEnumerable<IPolicyReportStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IPolicyReportStrategy Select(ReportDimension dimension)
        {
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(dimension));

            if (strategy is null)
                throw new InvalidOperationException($"No report strategy registered for dimension {dimension}.");

            return strategy;
        }
    }
}