using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Logging")]
public sealed class Peach : Fruit
{
    public override string Color => "Red/Yellow";

    [Attr]
    public double DiameterInCentimeters { get; set; }
}
