using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Capabilities;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Capabilities")]
public sealed class AllowAddRemoveCapability : Identifiable<long>
{
    [HasMany]
    public ISet<AllowAddRemoveCapability> ChildrenOn { get; set; } = new HashSet<AllowAddRemoveCapability>();

    [HasMany(Capabilities = ~HasManyCapabilities.AllowAdd)]
    public ISet<AllowAddRemoveCapability> ChildrenAddOff { get; set; } = new HashSet<AllowAddRemoveCapability>();

    [HasMany(Capabilities = ~HasManyCapabilities.AllowRemove)]
    public ISet<AllowAddRemoveCapability> ChildrenRemoveOff { get; set; } = new HashSet<AllowAddRemoveCapability>();
}
