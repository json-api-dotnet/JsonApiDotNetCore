using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite
{
    public sealed class WorkItemsController : JsonApiController<WorkItem, int>
    {
        public WorkItemsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<WorkItem, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
