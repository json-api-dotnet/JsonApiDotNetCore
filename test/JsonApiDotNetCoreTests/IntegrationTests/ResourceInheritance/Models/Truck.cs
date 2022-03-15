using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public sealed class Truck : MotorVehicle
{
    [Attr]
    public decimal LoadingCapacity { get; set; }

    [HasOne]
    public Box? SleepingArea { get; set; }

    [HasMany]
    public ISet<GenericFeature> Features { get; set; } = new HashSet<GenericFeature>();
}
