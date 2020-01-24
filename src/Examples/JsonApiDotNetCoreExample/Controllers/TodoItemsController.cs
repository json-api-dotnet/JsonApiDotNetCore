using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceService<TodoItem> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        {
        }
    }
}
