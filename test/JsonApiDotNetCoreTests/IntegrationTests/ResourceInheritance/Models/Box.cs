using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public sealed class Box : Identifiable<long>
{
    [Attr]
    public decimal Width { get; set; }

    [Attr]
    public decimal Height { get; set; }

    [Attr]
    public decimal Depth { get; set; }
}
