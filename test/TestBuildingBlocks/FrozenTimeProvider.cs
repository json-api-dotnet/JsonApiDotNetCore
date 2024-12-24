using System.Globalization;

namespace TestBuildingBlocks;

public sealed class FrozenTimeProvider(DateTimeOffset startDateTime, TimeZoneInfo? localTimeZone = null) : TimeProvider
{
    private readonly DateTimeOffset _startDateTime = startDateTime;
    private readonly TimeZoneInfo? _localTimeZone = localTimeZone;

    private DateTimeOffset _dateTime = startDateTime;

    public override TimeZoneInfo LocalTimeZone => _localTimeZone ?? throw new NotSupportedException();
    public override long TimestampFrequency => throw new NotSupportedException();

    public override DateTimeOffset GetUtcNow()
    {
        return _dateTime;
    }

    public void SetUtcNow(DateTimeOffset value)
    {
        _dateTime = value;
    }

    public void Reset()
    {
        _dateTime = _startDateTime;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        throw new NotSupportedException();
    }

    public override long GetTimestamp()
    {
        throw new NotSupportedException();
    }

    public override string ToString()
    {
        return _dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
    }
}
