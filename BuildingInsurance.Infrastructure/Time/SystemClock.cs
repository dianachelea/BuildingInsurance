using BuildingInsurance.Application.Features.Common.Abstractions;

namespace BuildingInsurance.Infrastructure.Time
{
    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
    }
}