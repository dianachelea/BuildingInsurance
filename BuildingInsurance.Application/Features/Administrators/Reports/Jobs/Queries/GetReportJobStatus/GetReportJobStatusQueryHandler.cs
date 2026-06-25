using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Queries.GetReportJobStatus
{
    public sealed class GetReportJobStatusQueryHandler : IRequestHandler<GetReportJobStatusQuery, Result<ReportJobStatusData>>
    {
        private readonly IReportJobsRepository _reportJobsRepository;
        private readonly ILogger<GetReportJobStatusQueryHandler> _logger;

        public GetReportJobStatusQueryHandler(IReportJobsRepository reportJobsRepository, ILogger<GetReportJobStatusQueryHandler> logger)
        {
            _reportJobsRepository = reportJobsRepository;
            _logger = logger;
        }

        public async Task<Result<ReportJobStatusData>> Handle(GetReportJobStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var status = await _reportJobsRepository.GetStatusAsync(request.JobId, cancellationToken);

                if (status is null)
                    return Result<ReportJobStatusData>.Failure("Report job not found.", ErrorType.NotFound);

                return Result<ReportJobStatusData>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during fetching report job status. JobId={JobId}", request.JobId);
                return Result<ReportJobStatusData>.Failure("Unexpected error during fetching report job status.", ErrorType.Generic);
            }
        }
    }
}