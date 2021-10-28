using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class TodoItemsController : JsonApiController<TodoItem, int>
    {
        public TodoItemsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<TodoItem, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
