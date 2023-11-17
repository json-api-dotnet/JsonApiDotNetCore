using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Logging")]
public sealed class FruitBowl : Identifiable<long>
{
    [HasMany]
    public ISet<Fruit> Fruits { get; set; } = new HashSet<Fruit>();
}
