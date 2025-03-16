using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ResourceInheritance")]
public sealed class District : Identifiable<Guid>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public ISet<Building> Buildings { get; set; } = new HashSet<Building>();

    [HasMany]
    public ISet<Road> Roads { get; set; } = new HashSet<Road>();
}
