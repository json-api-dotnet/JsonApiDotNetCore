using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(
            IJsonApiOptions jsonApiOPtions,
            IJsonApiContext jsonApiContext,
            IResourceService<TodoItem> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOPtions, jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
