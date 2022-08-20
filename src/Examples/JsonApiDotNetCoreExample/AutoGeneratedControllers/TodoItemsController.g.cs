using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Controllers;

public sealed partial class TodoItemsController : JsonApiController<TodoItem, int>
{
    public TodoItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<TodoItem, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
