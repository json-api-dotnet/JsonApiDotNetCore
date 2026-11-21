using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DiscoveryTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class PrivateCompoundResource : Identifiable<long>
{
    [Attr(IsCompound = true)]
    public PrivateCompound? Details { get; set; }
}
