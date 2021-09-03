using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Painting : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [HasOne]
        public ArtGallery ExposedAt { get; set; }
    }
}
