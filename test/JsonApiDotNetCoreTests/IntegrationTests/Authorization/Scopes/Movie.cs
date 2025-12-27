using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes")]
public sealed class Movie : Identifiable<long>
{
    [Attr]
    public required string Title { get; set; }

    [Attr]
    public int ReleaseYear { get; set; }

    [Attr]
    public int DurationInSeconds { get; set; }

    [HasOne]
    public required Genre Genre { get; set; }

    [HasMany]
    public ISet<Actor> Cast { get; set; } = new HashSet<Actor>();
}
