using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [ResourceLinks(TopLevelLinks = LinkTypes.None, ResourceLinks = LinkTypes.None, RelationshipLinks = LinkTypes.Related)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Links")]
    public sealed class PhotoLocation : Identifiable<int>
    {
        [Attr]
        public string? PlaceName { get; set; }

        [Attr]
        public double Latitude { get; set; }

        [Attr]
        public double Longitude { get; set; }

        [HasOne]
        public Photo Photo { get; set; } = null!;

        [HasOne(Links = LinkTypes.None)]
        public PhotoAlbum? Album { get; set; }
    }
}
