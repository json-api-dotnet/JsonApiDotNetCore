using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound")]
public abstract class Residence : Listing
{
    [Attr(IsCompound = true)]
    public ListingAddress? Address { get; set; }

    [Attr]
    public List<string> Photos { get; set; } = [];

    [Attr(IsCompound = true)]
    public List<ListingFeature> Features { get; set; } = [];

    [Attr(IsCompound = true)]
    public List<Room> Rooms { get; set; } = [];
}
