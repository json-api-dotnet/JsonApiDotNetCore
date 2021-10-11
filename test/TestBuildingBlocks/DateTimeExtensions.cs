#nullable disable

using System;

namespace TestBuildingBlocks
{
    public static class DateTimeExtensions
    {
        // The milliseconds precision in DateTime/DateTimeOffset values that fakers produce is higher
        // than what PostgreSQL can store. This results in our resource change tracker to detect
        // that the time stored in database differs from the time in the request body.
        // While that's technically correct, we don't want such side effects influencing our tests everywhere.

        public static DateTimeOffset TruncateToWholeMilliseconds(this DateTimeOffset value)
        {
            DateTime dateTime = TruncateToWholeMilliseconds(value.DateTime);

            // PostgreSQL is unable to store the timezone in the database, so it always uses the local zone.
            // See https://www.npgsql.org/doc/types/datetime.html.
            // We're taking the local zone here, to prevent our change tracker to detect that the time
            // stored in database differs from the time in the request body. While that's technically
            // correct, we don't want such side effects influencing our tests everywhere.
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);

            return new DateTimeOffset(dateTime, offset);
        }

        public static DateTime TruncateToWholeMilliseconds(this DateTime value)
        {
            long ticksToSubtract = value.Ticks % TimeSpan.TicksPerMillisecond;
            long ticksInWholeMilliseconds = value.Ticks - ticksToSubtract;

            return new DateTime(ticksInWholeMilliseconds, value.Kind);
        }
    }
}
