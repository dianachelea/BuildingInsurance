namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface IReportJobsRunner
    {
        Task<int> RunOnceAsync(DateTime nowUtc, CancellationToken ct);
    }
}