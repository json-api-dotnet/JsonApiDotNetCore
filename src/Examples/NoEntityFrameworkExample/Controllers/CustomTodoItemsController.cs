using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace NoEntityFrameworkExample.Controllers
{
    public class CustomTodoItemsController : JsonApiController<TodoItem>
    {
        public CustomTodoItemsController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext, 
            IResourceService<TodoItem> resourceService, 
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
