using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BuildingInsurance.Infrastructure.Repositories.PolicyReportsRepository
{
    public sealed class ReportJobsRepository : IReportJobsRepository
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly BuildingInsuranceDbContext _db;

        public ReportJobsRepository(BuildingInsuranceDbContext db) => _db = db;

        public async Task<Guid> CreateQueuedAsync(ReportDimension dimension, PolicyReportFilters filters, DateTime nowUtc, CancellationToken ct)
        {
            var jobId = Guid.NewGuid();

            var payload = new ReportJobPayload
            {
                Dimension = dimension,
                Filters = filters
            };

            var job = new ReportJob
            {
                Id = jobId,
                Status = "Queued",
                Progress = 0,
                CreatedAtUtc = nowUtc,
                PayloadJson = JsonSerializer.Serialize(payload, JsonOptions)
            };

            _db.ReportJobs.Add(job);
            await _db.SaveChangesAsync(ct);

            return jobId;
        }

        public async Task<ReportJobStatusData?> GetStatusAsync(Guid jobId, CancellationToken ct)
        {
            var job = await _db.ReportJobs
                .AsNoTracking()
                .Where(x => x.Id == jobId)
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    x.Progress,
                    x.Error,
                    x.ResultId
                })
                .SingleOrDefaultAsync(ct);

            if (job is null)
                return null;

            return new ReportJobStatusData(
                JobId: job.Id,
                Status: job.Status,
                Progress: job.Progress,
                Error: job.Error,
                ResultId: job.ResultId);
        }

        public async Task<ReportJobResultData?> GetResultAsync(Guid jobId, CancellationToken ct)
        {
            var job = await _db.ReportJobs
                .AsNoTracking()
                .Where(x => x.Id == jobId)
                .Select(x => new { x.Id, x.Status, x.Error, x.ResultId })
                .SingleOrDefaultAsync(ct);

            if (job is null)
                return null;

            if (job.Status == "Failed")
            {
                return new ReportJobResultData(
                    JobId: jobId,
                    IsReady: true,
                    IsFailed: true,
                    Error: job.Error,
                    Rows: null);
            }

            if (job.Status != "Succeeded" || job.ResultId is null)
            {
                return new ReportJobResultData(
                    JobId: jobId,
                    IsReady: false,
                    IsFailed: false,
                    Error: null,
                    Rows: null);
            }

            var result = await _db.ReportJobResults
                .AsNoTracking()
                .Where(r => r.Id == job.ResultId.Value)
                .Select(r => r.RowsJson)
                .SingleOrDefaultAsync(ct);

            if (result is null)
            {
                return new ReportJobResultData(
                    JobId: jobId,
                    IsReady: false,
                    IsFailed: false,
                    Error: null,
                    Rows: null);
            }

            var rows = JsonSerializer.Deserialize<List<PolicyReportRowDto>>(result, JsonOptions) ?? new();

            return new ReportJobResultData(
                JobId: jobId,
                IsReady: true,
                IsFailed: false,
                Error: null,
                Rows: rows);
        }

        public async Task<QueuedReportJob?> TryDequeueAsync(DateTime nowUtc, CancellationToken ct)
        {
            var job = await _db.ReportJobs
                .OrderBy(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(x => x.Status == "Queued", ct);

            if (job is null)
                return null;

            var payload = JsonSerializer.Deserialize<ReportJobPayload>(job.PayloadJson, JsonOptions);
            if (payload is null)
                return null;

            return new QueuedReportJob
            {
                JobId = job.Id,
                Dimension = payload.Dimension,
                Filters = payload.Filters
            };
        }

        public async Task MarkRunningAsync(Guid jobId, DateTime nowUtc, CancellationToken ct)
        {
            var job = await _db.ReportJobs.SingleAsync(x => x.Id == jobId, ct);
            job.Status = "Running";
            job.Progress = 1;
            job.StartedAtUtc = nowUtc;
            job.Error = null;

            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkProgressAsync(Guid jobId, int progress, CancellationToken ct)
        {
            var job = await _db.ReportJobs.SingleAsync(x => x.Id == jobId, ct);
            job.Progress = Math.Clamp(progress, 0, 100);
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkFailedAsync(Guid jobId, string error, DateTime nowUtc, CancellationToken ct)
        {
            var job = await _db.ReportJobs.SingleAsync(x => x.Id == jobId, ct);

            job.Status = "Failed";
            job.Progress = 100;
            job.Error = string.IsNullOrWhiteSpace(error) ? "Unknown error" : error;
            job.FinishedAtUtc = nowUtc;

            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkSucceededAsync(Guid jobId, Guid resultId, DateTime nowUtc, CancellationToken ct)
        {
            var job = await _db.ReportJobs.SingleAsync(x => x.Id == jobId, ct);

            job.Status = "Succeeded";
            job.Progress = 100;
            job.ResultId = resultId;
            job.FinishedAtUtc = nowUtc;
            job.Error = null;

            await _db.SaveChangesAsync(ct);
        }

        public async Task<Guid> SaveResultAsync(Guid jobId, List<PolicyReportRowDto> rows, DateTime nowUtc, CancellationToken ct)
        {
            var resultId = Guid.NewGuid();

            _db.ReportJobResults.Add(new ReportJobResult
            {
                Id = resultId,
                JobId = jobId,
                CreatedAtUtc = nowUtc,
                RowsJson = JsonSerializer.Serialize(rows ?? new List<PolicyReportRowDto>(), JsonOptions)
            });

            await _db.SaveChangesAsync(ct);
            return resultId;
        }
    }
}