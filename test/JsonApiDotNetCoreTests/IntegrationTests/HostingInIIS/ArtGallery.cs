using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS")]
public sealed class ArtGallery : Identifiable<long>
{
    [Attr]
    public required string Theme { get; set; }

    [HasMany]
    public ISet<Painting> Paintings { get; set; } = new HashSet<Painting>();
}
