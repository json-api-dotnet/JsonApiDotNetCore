using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ArtGallery : Identifiable
    {
        [Attr]
        public string Theme { get; set; }

        [HasMany]
        public ISet<Painting> Paintings { get; set; }
    }
}
