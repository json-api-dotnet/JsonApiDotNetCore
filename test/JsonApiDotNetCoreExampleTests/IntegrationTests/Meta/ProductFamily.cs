using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class ProductFamily : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public IList<SupportTicket> Tickets { get; set; }
    }
}
