using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Logging")]
public abstract class Fruit : Identifiable<long>
{
    [Attr]
    public abstract string Color { get; }

    [Attr]
    public double WeightInKilograms { get; set; }
}
