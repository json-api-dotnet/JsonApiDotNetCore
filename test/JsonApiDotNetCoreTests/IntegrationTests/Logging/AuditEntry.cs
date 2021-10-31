using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class AuditEntry : Identifiable<int>
    {
        [Attr]
        public string UserName { get; set; } = null!;

        [Attr]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
