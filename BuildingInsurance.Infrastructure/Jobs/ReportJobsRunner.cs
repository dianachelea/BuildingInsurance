using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Infrastructure.Jobs
{
    public sealed class ReportJobsRunner : IReportJobsRunner
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReportJobsRepository _jobsRepository;
        private readonly IReportJobProcessor _processor;
        private readonly ILogger<ReportJobsRunner> _logger;

        public ReportJobsRunner(IUnitOfWork unitOfWork, IReportJobsRepository jobsRepository, IReportJobProcessor processor, ILogger<ReportJobsRunner> logger)
        {
            _unitOfWork = unitOfWork;
            _jobsRepository = jobsRepository;
            _processor = processor;
            _logger = logger;
        }

        public async Task<int> RunOnceAsync(DateTime nowUtc, CancellationToken ct)
        {
            var queued = await _jobsRepository.TryDequeueAsync(nowUtc, ct);
            if (queued is null)
                return 0;

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(ct);
                transactionStarted = true;

                await _jobsRepository.MarkRunningAsync(queued.JobId, nowUtc, ct);
                await _jobsRepository.MarkProgressAsync(queued.JobId, 10, ct);

                List<PolicyReportRowDto> rows;

                try
                {
                    rows = await _processor.GenerateAsync(queued.Dimension, queued.Filters, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Report job failed while generating. JobId={JobId}", queued.JobId);

                    await _jobsRepository.MarkFailedAsync(queued.JobId, error: "Failed to generate report.", nowUtc: nowUtc, ct: ct);

                    await _unitOfWork.CommitAsync(ct);
                    committed = true;

                    return 1;
                }

                await _jobsRepository.MarkProgressAsync(queued.JobId, 80, ct);

                var resultId = await _jobsRepository.SaveResultAsync(queued.JobId, rows, nowUtc, ct);

                await _jobsRepository.MarkSucceededAsync(queued.JobId, resultId, nowUtc, ct);

                await _unitOfWork.CommitAsync(ct);
                committed = true;

                _logger.LogInformation("Report job succeeded. JobId={JobId} Rows={Rows}", queued.JobId, rows.Count);

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing report job. JobId={JobId}", queued.JobId);

                try
                {
                    await _jobsRepository.MarkFailedAsync(queued.JobId, error: "Unexpected error while processing report job.", nowUtc: nowUtc, ct: ct);
                }
                catch (Exception markEx)
                {
                    _logger.LogWarning(markEx, "Failed to mark report job as failed. JobId={JobId}", queued.JobId);
                }

                throw;
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        await _unitOfWork.RollbackAsync(ct);
                    }
                    catch (Exception rbEx)
                    {
                        _logger.LogWarning(rbEx, "Rollback failed in ReportJobsRunner.");
                    }
                }
            }
        }
    }
}