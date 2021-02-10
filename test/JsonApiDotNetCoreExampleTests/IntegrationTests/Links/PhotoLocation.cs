using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    [ResourceLinks(TopLevelLinks = LinkTypes.None, ResourceLinks = LinkTypes.None, RelationshipLinks = LinkTypes.Related)]
    public sealed class PhotoLocation : Identifiable
    {
        [Attr]
        public string PlaceName { get; set; }

        [Attr]
        public double Latitude { get; set; }

        [Attr]
        public double Longitude { get; set; }

        [HasOne]
        public Photo Photo { get; set; }

        [HasOne(Links = LinkTypes.None)]
        public PhotoAlbum Album { get; set; }
    }
}
