using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class Blog : Identifiable
    {
        [Attr] 
        public string Title { get; set; }

        [Attr]
        public string PlatformName { get; set; }

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange))]
        public bool ShowAdvertisements => PlatformName.EndsWith("(using free account)");

        [HasMany]
        public IList<BlogPost> Posts { get; set; }

        [HasOne]
        public WebAccount Owner { get; set; }
    }
}
