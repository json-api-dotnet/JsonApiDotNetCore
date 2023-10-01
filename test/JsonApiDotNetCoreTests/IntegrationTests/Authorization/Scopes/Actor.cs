using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes")]
public sealed class Actor : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public DateTime BornAt { get; set; }

    [HasMany]
    public ISet<Movie> ActsIn { get; set; } = new HashSet<Movie>();
}
