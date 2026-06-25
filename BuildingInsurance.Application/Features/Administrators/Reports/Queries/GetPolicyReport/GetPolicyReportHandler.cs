using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Selection;
using BuildingInsurance.Application.Features.Common.Extensions;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Queries.GetPolicyReport
{
    public sealed class GetPolicyReportHandler : IRequestHandler<GetPolicyReportQuery, Result<GetPolicyReportResponse>>
    {
        private readonly IPolicyReportStrategySelector _selector;
        private readonly ILogger<GetPolicyReportHandler> _logger;

        public GetPolicyReportHandler(IPolicyReportStrategySelector selector, ILogger<GetPolicyReportHandler> logger)
        {
            _selector = selector;
            _logger = logger;
        }

        public async Task<Result<GetPolicyReportResponse>> Handle(GetPolicyReportQuery request, CancellationToken cancellationToken)
        {
            var filters = request.Filters;
            try
            {
                var strategy = _selector.Select(request.Dimension);
                _logger.LogInformation("Selected report strategy for dimension {Dimension}", request.Dimension);

                var rows = await strategy.GenerateReportAsync(
                    from: filters.From.ToUtc(),
                    to: filters.To.ToUtc(),
                    status: filters.Status.HasValue ? filters.Status.Value.MapToDomainPolicyStatus() : default,
                    currencyCode: filters.CurrencyCode,
                    buildingType: filters.BuildingType.MapToDomainBuildingTypeOptional(),
                    ct: cancellationToken);

                rows ??= new List<PolicyReportRowDto>();

                var totalCount = rows.Count;
                var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
                var items = rows
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var response = new GetPolicyReportResponse(items, totalPages, totalCount);

                _logger.LogInformation("Report generated successfully. Dimension={Dimension}, TotalCount={TotalCount}, Page={Page}, PageSize={PageSize}", request.Dimension, totalCount, request.Page, request.PageSize);

                return Result<GetPolicyReportResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate policy report. Dimension={Dimension}", request.Dimension);
                return Result<GetPolicyReportResponse>.Failure("Unexpected error during report generating.", ErrorType.Generic);
            }
        }
    }
}