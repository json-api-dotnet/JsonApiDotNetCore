using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ProductFamily : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasMany]
        public IList<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
    }
}
