using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class Photo : Identifiable<Guid>
    {
        [Attr]
        public string Url { get; set; }

        [Attr]
        public Guid ConcurrencyToken => Guid.NewGuid();

        [HasOne]
        public PhotoAlbum Album { get; set; }
    }
}
