using BuildingInsurance.Application.Features.Administrators.Reports.Common.Dtos;
using BuildingInsurance.Application.Features.Administrators.Reports.Common.Models;
using BuildingInsurance.Application.Features.Common.Models;

namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IReportJobsRepository
    {
        Task<Guid> CreateQueuedAsync(ReportDimension dimension, PolicyReportFilters filters, DateTime nowUtc, CancellationToken ct);
        Task<ReportJobStatusData?> GetStatusAsync(Guid jobId, CancellationToken ct);
        Task<ReportJobResultData?> GetResultAsync(Guid jobId, CancellationToken ct);
        Task<QueuedReportJob?> TryDequeueAsync(DateTime nowUtc, CancellationToken ct);
        Task MarkRunningAsync(Guid jobId, DateTime nowUtc, CancellationToken ct);
        Task MarkProgressAsync(Guid jobId, int progress, CancellationToken ct);
        Task MarkFailedAsync(Guid jobId, string error, DateTime nowUtc, CancellationToken ct);
        Task MarkSucceededAsync(Guid jobId, Guid resultId, DateTime nowUtc, CancellationToken ct);
        Task<Guid> SaveResultAsync(Guid jobId, List<PolicyReportRowDto> rows, DateTime nowUtc, CancellationToken ct);
    }
}