using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(
            IJsonApiOptions jsonApiOPtions,
            IResourceGraph resourceGraph,
            IResourceService<TodoItem> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOPtions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
