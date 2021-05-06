using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class EngagementParty : EntityBase<Guid>
    {
        // Data (simplified)

        [Attr]
        public string Role { get; set; }

        [Attr]
        public string ShortName { get; set; }

        // Foreign Keys (simplified)

        [HasOne]
        public Engagement Engagement { get; set; }

        //public Guid EngagementId { get; set; }
    }
}
