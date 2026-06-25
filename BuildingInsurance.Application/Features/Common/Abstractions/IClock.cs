namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateOnly TodayUtc { get; }
    }
}