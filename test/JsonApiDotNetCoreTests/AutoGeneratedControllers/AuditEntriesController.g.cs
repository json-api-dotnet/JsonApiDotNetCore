using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

public sealed partial class AuditEntriesController : JsonApiController<AuditEntry, int>
{
    public AuditEntriesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<AuditEntry, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
