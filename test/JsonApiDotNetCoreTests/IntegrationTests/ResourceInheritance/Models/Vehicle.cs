using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public abstract class Vehicle : Identifiable<long>
{
    [Attr]
    public decimal Weight { get; set; }

    [Attr]
    public abstract bool RequiresDriverLicense { get; set; }

    [HasOne]
    public VehicleManufacturer? Manufacturer { get; set; }

    [HasMany]
    public ISet<Wheel> Wheels { get; set; } = new HashSet<Wheel>();
}
