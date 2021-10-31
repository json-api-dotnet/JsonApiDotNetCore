using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class SupportTicketDefinition : JsonApiResourceDefinition<SupportTicket, int>
    {
        private readonly ResourceDefinitionHitCounter _hitCounter;

        public SupportTicketDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            _hitCounter = hitCounter;
        }

        public override IDictionary<string, object?>? GetMeta(SupportTicket resource)
        {
            _hitCounter.TrackInvocation<SupportTicket>(ResourceDefinitionHitCounter.ExtensibilityPoint.GetMeta);

            if (!string.IsNullOrEmpty(resource.Description) && resource.Description.StartsWith("Critical:", StringComparison.Ordinal))
            {
                return new Dictionary<string, object?>
                {
                    ["hasHighPriority"] = true
                };
            }

            return base.GetMeta(resource);
        }
    }
}
