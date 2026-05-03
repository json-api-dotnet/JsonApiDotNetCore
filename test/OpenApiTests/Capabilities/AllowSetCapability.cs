using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Capabilities")]
public sealed class AllowSetCapability : Identifiable<long>
{
    [HasOne]
    public AllowSetCapability? ParentSetOn { get; set; }

    [HasOne(Capabilities = ~HasOneCapabilities.AllowSet)]
    public AllowSetCapability? ParentSetOff { get; set; }

    [HasMany]
    public ISet<AllowSetCapability> ChildrenSetOn { get; set; } = new HashSet<AllowSetCapability>();

    [HasMany(Capabilities = ~HasManyCapabilities.AllowSet)]
    public ISet<AllowSetCapability> ChildrenSetOff { get; set; } = new HashSet<AllowSetCapability>();
}
