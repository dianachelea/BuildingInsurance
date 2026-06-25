namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface IPolicyExpirationService
    {
        Task<int> RunOnceAsync(DateTime nowUtc, CancellationToken ct = default);
    }
}