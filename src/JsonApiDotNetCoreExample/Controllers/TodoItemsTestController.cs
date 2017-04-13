using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public abstract class AbstractTodoItemsController<T> : JsonApiController<T> where T : class, IIdentifiable<int>
    {
        protected AbstractTodoItemsController(
            IJsonApiContext jsonApiContext,
            IResourceService<T, int> service,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, service, loggerFactory)
        {
        }
    }
    public class TodoItemsTestController : AbstractTodoItemsController<TodoItem>
    {
        public TodoItemsTestController(
            IJsonApiContext jsonApiContext,
            IResourceService<TodoItem> service,
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, service, loggerFactory)
        { }
    }
}
