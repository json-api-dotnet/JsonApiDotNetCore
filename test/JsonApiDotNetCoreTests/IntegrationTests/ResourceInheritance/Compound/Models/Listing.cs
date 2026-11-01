using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound")]
public abstract class Listing : Identifiable<long>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public decimal Price { get; set; }

    [HasOne]
    public ListingAgent? Agent { get; set; }
}
