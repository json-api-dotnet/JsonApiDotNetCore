using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Controllers
{
    public sealed class WorkItemsController : JsonApiController<WorkItem, int>
    {
        public WorkItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<WorkItem, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
