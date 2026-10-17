using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace DiscoveryTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class PrivateCompound
{
    [Attr]
    public string? Name { get; set; }

    [Attr]
    public int Count { get; set; }
}
