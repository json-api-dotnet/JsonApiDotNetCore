using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Logging
{
    public sealed class AuditEntriesController : JsonApiController<AuditEntry>
    {
        public AuditEntriesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<AuditEntry> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
