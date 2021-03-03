using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class SupportTicketDefinition : JsonApiResourceDefinition<SupportTicket>
    {
        public SupportTicketDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }

        public override IDictionary<string, object> GetMeta(SupportTicket resource)
        {
            if (resource.Description != null && resource.Description.StartsWith("Critical:", StringComparison.Ordinal))
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
