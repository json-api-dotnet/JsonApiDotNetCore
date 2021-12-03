using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS")]
    public sealed class ArtGallery : Identifiable<int>
    {
        [Attr]
        public string Theme { get; set; } = null!;

        [HasMany]
        public ISet<Painting> Paintings { get; set; } = new HashSet<Painting>();
    }
}
