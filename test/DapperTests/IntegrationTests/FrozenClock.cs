using DapperExample;
using FluentAssertions.Extensions;
using JetBrains.Annotations;

namespace DapperTests.IntegrationTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class FrozenClock : IClock
{
    private static readonly DateTimeOffset DefaultTime = 1.January(2020).At(1, 1, 1).AsUtc();

    public DateTimeOffset UtcNow { get; set; } = DefaultTime;
}
