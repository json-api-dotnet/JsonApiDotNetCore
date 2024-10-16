using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance", GenerateControllerEndpoints = JsonApiEndpoints.GetCollection)]
public sealed class District : Identifiable<Guid>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public ISet<Building> Buildings { get; set; } = new HashSet<Building>();
}
