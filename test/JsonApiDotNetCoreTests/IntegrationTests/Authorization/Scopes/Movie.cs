using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes")]
public sealed class Movie : Identifiable<long>
{
    [Attr]
    public string Title { get; set; } = null!;

    [Attr]
    public int ReleaseYear { get; set; }

    [Attr]
    public int DurationInSeconds { get; set; }

    [HasOne]
    public Genre Genre { get; set; } = null!;

    [HasMany]
    public ISet<Actor> Cast { get; set; } = new HashSet<Actor>();
}
