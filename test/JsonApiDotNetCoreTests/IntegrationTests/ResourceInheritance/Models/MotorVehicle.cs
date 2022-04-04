using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public abstract class MotorVehicle : Vehicle
{
    [Attr]
    public override bool RequiresDriverLicense { get; set; }

    [Attr]
    public string LicensePlate { get; set; } = null!;

    [HasOne]
    public Engine Engine { get; set; } = null!;

    [HasOne]
    public NavigationSystem? NavigationSystem { get; set; }
}
