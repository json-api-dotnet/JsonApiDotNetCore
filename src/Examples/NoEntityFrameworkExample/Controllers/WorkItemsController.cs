using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using NoEntityFrameworkExample.Models;
using Microsoft.Extensions.Logging;

namespace NoEntityFrameworkExample.Controllers
{
    public sealed class WorkItemsController : JsonApiController<WorkItem>
    {
        public WorkItemsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<WorkItem> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
