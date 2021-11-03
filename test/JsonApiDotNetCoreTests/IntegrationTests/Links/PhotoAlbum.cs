using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class PhotoAlbum : Identifiable<Guid>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasMany]
        public ISet<Photo> Photos { get; set; } = new HashSet<Photo>();
    }
}
