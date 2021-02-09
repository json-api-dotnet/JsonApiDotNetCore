using System;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Logging
{
    public sealed class AuditEntry : Identifiable
    {
        [Attr]
        public string UserName { get; set; }

        [Attr]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
