using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Cosmos.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Cosmos.Controllers
{
    public sealed class TodoItemsController : JsonApiController<TodoItem, Guid>
    {
        public TodoItemsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<TodoItem, Guid> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
