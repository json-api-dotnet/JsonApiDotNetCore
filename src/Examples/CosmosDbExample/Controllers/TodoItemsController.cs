using System;
using CosmosDbExample.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace CosmosDbExample.Controllers
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
