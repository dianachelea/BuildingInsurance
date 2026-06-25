namespace BuildingInsurance.Application.Features.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;

            if (dateTime.Kind == DateTimeKind.Local)
                return dateTime.ToUniversalTime();

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
        }
    }
}