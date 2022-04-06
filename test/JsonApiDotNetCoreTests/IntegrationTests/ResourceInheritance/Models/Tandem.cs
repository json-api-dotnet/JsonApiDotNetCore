using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance")]
public sealed class Tandem : Bike
{
    [Attr]
    public int PassengerCount { get; set; }

    [HasMany]
    public ISet<GenericFeature> Features { get; set; } = new HashSet<GenericFeature>();
}
