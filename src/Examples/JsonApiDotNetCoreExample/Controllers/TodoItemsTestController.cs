using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public abstract class AbstractTodoItemsController<T> 
    : JsonApiController<T> where T : class, IIdentifiable<int>
    {
        protected AbstractTodoItemsController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceService<T, int> service)
            : base(jsonApiOptions, loggerFactory, service)
        {
        }
    }

    [Route("/abstract")]
    public class TodoItemsTestController : AbstractTodoItemsController<TodoItem>
    {
        public TodoItemsTestController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceService<TodoItem> service)
            : base(jsonApiOptions, loggerFactory, service)
        {
        }
    }
}
