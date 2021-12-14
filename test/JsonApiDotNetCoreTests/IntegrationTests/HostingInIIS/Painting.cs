using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS")]
public sealed class Painting : Identifiable<int>
{
    [Attr]
    public string Title { get; set; } = null!;

    [HasOne]
    public ArtGallery? ExposedAt { get; set; }
}
