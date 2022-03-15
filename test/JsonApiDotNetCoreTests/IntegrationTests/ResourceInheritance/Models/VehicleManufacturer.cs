using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public sealed class VehicleManufacturer : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public ISet<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
}
