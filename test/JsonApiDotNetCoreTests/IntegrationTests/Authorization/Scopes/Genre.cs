using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes")]
public sealed class Genre : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public ISet<Movie> Movies { get; set; } = new HashSet<Movie>();
}
