using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AuditEntry : Identifiable
    {
        [Attr]
        public string UserName { get; set; }

        [Attr]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
