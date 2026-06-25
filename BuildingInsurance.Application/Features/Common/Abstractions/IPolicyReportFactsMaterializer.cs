namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface IPolicyReportFactsMaterializer
    {
        Task<int> RunOnceAsync(DateTime nowUtc, CancellationToken ct);
    }
}