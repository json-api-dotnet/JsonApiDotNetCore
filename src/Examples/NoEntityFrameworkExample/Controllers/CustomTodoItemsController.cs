using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace NoEntityFrameworkExample.Controllers
{
    public class CustomTodoItemsController : JsonApiController<TodoItem>
    {
        public CustomTodoItemsController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph, 
            IResourceService<TodoItem> resourceService, 
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
