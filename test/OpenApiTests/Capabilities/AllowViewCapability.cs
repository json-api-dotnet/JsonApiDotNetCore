using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Capabilities")]
public sealed class AllowViewCapability : Identifiable<long>
{
    [Attr]
    public string? AttributeViewOn { get; set; }

    [Attr(Capabilities = ~AttrCapabilities.AllowView)]
    public string? AttributeViewOff { get; set; }

    [HasOne]
    public AllowViewCapability? ParentViewOn { get; set; }

    [HasOne(Capabilities = ~HasOneCapabilities.AllowView)]
    public AllowViewCapability? ParentViewOff { get; set; }

    [HasMany]
    public ISet<AllowViewCapability> ChildrenViewOn { get; set; } = new HashSet<AllowViewCapability>();

    [HasMany(Capabilities = ~HasManyCapabilities.AllowView)]
    public ISet<AllowViewCapability> ChildrenViewOff { get; set; } = new HashSet<AllowViewCapability>();
}
