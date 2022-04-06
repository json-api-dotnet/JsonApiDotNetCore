using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public class Bike : Vehicle
{
    [Attr]
    public override bool RequiresDriverLicense { get; set; }

    [Attr]
    public int GearCount { get; set; }

    [HasOne]
    public Box? CargoBox { get; set; }

    [HasMany]
    public ISet<BicycleLight> Lights { get; set; } = new HashSet<BicycleLight>();
}
