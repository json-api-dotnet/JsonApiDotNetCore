using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(
            IJsonApiContext jsonApiContext,
            IResourceService<TodoItem> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
