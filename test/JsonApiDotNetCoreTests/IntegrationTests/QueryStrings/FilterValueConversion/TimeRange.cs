namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.FilterValueConversion;

internal sealed class TimeRange
{
    public DateTime Time { get; }
    public TimeSpan Offset { get; }

    public TimeRange(DateTime time, TimeSpan offset)
    {
        Time = time;
        Offset = offset;
    }
}
