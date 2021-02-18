using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Controllers
{
    public sealed class WorkItemsController : JsonApiController<WorkItem>
    {
        public WorkItemsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<WorkItem> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
