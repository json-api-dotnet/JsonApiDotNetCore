using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using NoEntityFrameworkExample.Models;
using Microsoft.Extensions.Logging;

namespace NoEntityFrameworkExample.Controllers
{
    public class CustomTodoItemsController : JsonApiController<TodoItem>
    {
        public CustomTodoItemsController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<TodoItem> resourceService, 
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceService, loggerFactory)
        { }
    }
}
