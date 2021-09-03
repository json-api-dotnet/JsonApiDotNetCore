using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Blog : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public string PlatformName { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public bool ShowAdvertisements => PlatformName.EndsWith("(using free account)", StringComparison.Ordinal);

        [HasMany]
        public IList<BlogPost> Posts { get; set; }

        [HasOne]
        public WebAccount Owner { get; set; }
    }
}
