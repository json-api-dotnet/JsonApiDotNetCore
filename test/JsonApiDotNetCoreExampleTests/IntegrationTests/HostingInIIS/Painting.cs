using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    public sealed class Painting : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [HasOne]
        public ArtGallery ExposedAt { get; set; }
    }
}
