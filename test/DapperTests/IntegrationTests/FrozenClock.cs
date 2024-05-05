using DapperExample;
using JetBrains.Annotations;
using TestBuildingBlocks;

namespace DapperTests.IntegrationTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class FrozenClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = FrozenSystemClock.DefaultDateTimeOffsetUtc;
}
