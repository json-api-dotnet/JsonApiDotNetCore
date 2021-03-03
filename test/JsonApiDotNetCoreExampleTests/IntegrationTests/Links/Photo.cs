using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Photo : Identifiable<Guid>
    {
        [Attr]
        public string Url { get; set; }

        [HasOne]
        public PhotoLocation Location { get; set; }

        [HasOne]
        public PhotoAlbum Album { get; set; }
    }
}
