using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Reports.Jobs.Queries.GetReportJobResultQuery
{
    public sealed class GetReportJobResultQueryHandler : IRequestHandler<GetReportJobResultQuery, Result<List<PolicyReportRowDto>>>
    {
        private readonly IReportJobsRepository _reportJobsRepository;
        private readonly ILogger<GetReportJobResultQueryHandler> _logger;

        public GetReportJobResultQueryHandler(IReportJobsRepository reportJobsRepository, ILogger<GetReportJobResultQueryHandler> logger)
        {
            _reportJobsRepository = reportJobsRepository;
            _logger = logger;
        }

        public async Task<Result<List<PolicyReportRowDto>>> Handle(GetReportJobResultQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _reportJobsRepository.GetResultAsync(request.JobId, cancellationToken);

                if (data is null)
                    return Result<List<PolicyReportRowDto>>.Failure("Report job not found.", ErrorType.NotFound);

                if (data.IsFailed)
                    return Result<List<PolicyReportRowDto>>.Failure(data.Error ?? "Report job failed.", ErrorType.Validation);

                if (!data.IsReady)
                    return Result<List<PolicyReportRowDto>>.Failure("Report job is not completed yet.", ErrorType.Conflict);

                return Result<List<PolicyReportRowDto>>.Success(data.Rows ?? new List<PolicyReportRowDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during fetching report job result. JobId={JobId}", request.JobId);
                return Result<List<PolicyReportRowDto>>.Failure("Unexpected error during fetching report job result.", ErrorType.Generic);
            }
        }
    }
}