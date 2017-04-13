using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using NoEntityFrameworkExample.Models;

namespace NoEntityFrameworkExample.Controllers
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
