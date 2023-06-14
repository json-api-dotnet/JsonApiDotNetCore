using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.EagerLoading")]
public sealed class City : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public IList<Street> Streets { get; set; } = new List<Street>();
}
