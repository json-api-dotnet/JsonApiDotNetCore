using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<TodoItem> entityRepository,
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }
}
