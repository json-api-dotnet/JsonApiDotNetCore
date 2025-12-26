using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Links")]
public sealed class PhotoAlbum : Identifiable<Guid>
{
    [Attr]
    public required string Name { get; set; }

    [HasMany]
    public ISet<Photo> Photos { get; set; } = new HashSet<Photo>();
}
