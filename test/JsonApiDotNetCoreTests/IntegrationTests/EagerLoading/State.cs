using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.EagerLoading")]
public sealed class State : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public IList<City> Cities { get; set; } = new List<City>();
}
