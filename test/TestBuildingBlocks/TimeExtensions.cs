namespace TestBuildingBlocks;

public static class TimeExtensions
{
    // The milliseconds precision in DateTime/DateTimeOffset/TimeSpan/TimeOnly values that fakers produce
    // is higher than what PostgreSQL can store. This results in our resource change tracker to detect
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
        long ticksInWholeMilliseconds = TruncateTicksInWholeMilliseconds(value.Ticks);
        return new DateTime(ticksInWholeMilliseconds, value.Kind);
    }

    private static long TruncateTicksInWholeMilliseconds(long ticks)
    {
        long ticksToSubtract = ticks % TimeSpan.TicksPerMillisecond;
        return ticks - ticksToSubtract;
    }

    public static TimeSpan TruncateToWholeMilliseconds(this TimeSpan value)
    {
        long ticksInWholeMilliseconds = TruncateTicksInWholeMilliseconds(value.Ticks);
        return new TimeSpan(ticksInWholeMilliseconds);
    }

    public static TimeOnly TruncateToWholeMilliseconds(this TimeOnly value)
    {
        long ticksInWholeMilliseconds = TruncateTicksInWholeMilliseconds(value.Ticks);
        return new TimeOnly(ticksInWholeMilliseconds);
    }

    public static TimeOnly TruncateToWholeSeconds(this TimeOnly value)
    {
        long ticksInWholeSeconds = TruncateTicksInWholeSeconds(value.Ticks);
        return new TimeOnly(ticksInWholeSeconds);
    }

    private static long TruncateTicksInWholeSeconds(long ticks)
    {
        long ticksToSubtract = ticks % TimeSpan.TicksPerSecond;
        return ticks - ticksToSubtract;
    }
}
