namespace TestBuildingBlocks;

public static class DateTimeExtensions
{
    // The milliseconds precision in DateTime/DateTimeOffset values that fakers produce is higher
    // than what PostgreSQL can store. This results in our resource change tracker to detect
    // that the time stored in the database differs from the time in the request body. While that's
    // technically correct, we don't want such side effects influencing our tests everywhere.

    public static DateTimeOffset TruncateToWholeMilliseconds(this DateTimeOffset value)
    {
        // Because PostgreSQL does not store the UTC offset in the database, it cannot round-trip
        // values with a non-zero UTC offset, and therefore always rejects such values.

        DateTime dateTime = TruncateToWholeMilliseconds(value.DateTime);
        return new DateTimeOffset(dateTime, TimeSpan.Zero);
    }

    public static DateTime TruncateToWholeMilliseconds(this DateTime value)
    {
        long ticksToSubtract = value.Ticks % TimeSpan.TicksPerMillisecond;
        long ticksInWholeMilliseconds = value.Ticks - ticksToSubtract;

        return new DateTime(ticksInWholeMilliseconds, value.Kind);
    }
}
