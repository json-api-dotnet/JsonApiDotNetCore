using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance", GenerateControllerEndpoints = JsonApiEndpoints.None)]
public sealed class Mansion : Residence
{
    [Attr]
    public string OwnerName { get; set; } = null!;
}
