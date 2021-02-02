using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class PhotoAlbum : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public Guid ConcurrencyToken => Guid.NewGuid();

        [HasMany]
        public ISet<Photo> Photos { get; set; }
    }
}
