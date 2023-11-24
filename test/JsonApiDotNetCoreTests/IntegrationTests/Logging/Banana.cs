using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Logging")]
public sealed class Banana : Fruit
{
    public override string Color => "Yellow";

    [Attr]
    public double LengthInCentimeters { get; set; }
}
