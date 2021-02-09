using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class SupportTicketDefinition : JsonApiResourceDefinition<SupportTicket>
    {
        public SupportTicketDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
        {
        }

        public override IDictionary<string, object> GetMeta(SupportTicket resource)
        {
            if (resource.Description != null && resource.Description.StartsWith("Critical:"))
            {
                return new Dictionary<string, object>
                {
                    ["hasHighPriority"] = true
                };
            }
            
            return base.GetMeta(resource);
        }
    }
}
