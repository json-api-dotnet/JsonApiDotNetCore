using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Blog : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = null!;

        [Attr]
        public string PlatformName { get; set; } = null!;

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public bool ShowAdvertisements => PlatformName.EndsWith("(using free account)", StringComparison.Ordinal);

        [HasMany]
        public IList<BlogPost> Posts { get; set; } = new List<BlogPost>();

        [HasOne]
        public WebAccount Owner { get; set; } = null!;
    }
}
