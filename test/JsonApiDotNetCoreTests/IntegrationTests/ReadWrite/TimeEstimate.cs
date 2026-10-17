using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TimeEstimate
{
    [Attr]
    public TimeSpan? Optimistic { get; set; }

    [Attr]
    public TimeSpan? Likely { get; set; }

    [Attr]
    public TimeSpan? Pessimistic { get; set; }
}
