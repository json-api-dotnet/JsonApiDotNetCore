using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS")]
public sealed class Painting : Identifiable<long>
{
    [Attr]
    public required string Title { get; set; }

    [HasOne]
    public ArtGallery? ExposedAt { get; set; }
}
