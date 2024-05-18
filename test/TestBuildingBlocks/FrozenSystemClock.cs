using FluentAssertions.Extensions;

namespace TestBuildingBlocks;

public sealed class FrozenSystemClock : ISystemClock
{
    public static readonly DateTime DefaultDateTimeUtc = 1.January(2020).At(1, 1, 1).AsUtc();
    public static readonly DateTimeOffset DefaultDateTimeOffsetUtc = DefaultDateTimeUtc;

    public DateTimeOffset UtcNow { get; set; } = DefaultDateTimeOffsetUtc;
}
