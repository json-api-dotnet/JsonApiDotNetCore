using DapperExample;
using FluentAssertions.Extensions;

namespace DapperTests.IntegrationTests;

internal sealed class FrozenClock : IClock
{
    private static readonly DateTimeOffset DefaultTime = 1.January(2020).At(1, 1, 1).AsUtc();

    public DateTimeOffset UtcNow { get; set; } = DefaultTime;
}
