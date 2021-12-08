using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Links")]
public sealed class Photo : Identifiable<Guid>
{
    [Attr]
    public string? Url { get; set; }

    [HasOne]
    public PhotoLocation? Location { get; set; }

    [HasOne]
    public PhotoAlbum? Album { get; set; }
}
