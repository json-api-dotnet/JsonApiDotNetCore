using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ProductFamily : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [HasMany]
        public IList<SupportTicket> Tickets { get; set; }
    }
}
