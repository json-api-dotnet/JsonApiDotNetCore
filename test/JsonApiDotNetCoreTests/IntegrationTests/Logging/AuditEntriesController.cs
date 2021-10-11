#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging
{
    public sealed class AuditEntriesController : JsonApiController<AuditEntry, int>
    {
        public AuditEntriesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<AuditEntry, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
