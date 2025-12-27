using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.EagerLoading")]
public sealed class City : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public IList<Street> Streets { get; set; } = new List<Street>();
}
