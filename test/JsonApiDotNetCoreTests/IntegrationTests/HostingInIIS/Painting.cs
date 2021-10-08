using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Painting : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; }

        [HasOne]
        public ArtGallery ExposedAt { get; set; }
    }
}
