using FluentAssertions.Extensions;

namespace TestBuildingBlocks;

public sealed class FrozenSystemClock : ISystemClock
{
    private static readonly DateTimeOffset DefaultTime = 1.January(2020).At(1, 1, 1).AsUtc();

    public DateTimeOffset UtcNow { get; set; } = DefaultTime;
}
