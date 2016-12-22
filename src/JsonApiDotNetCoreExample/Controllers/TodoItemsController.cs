using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCoreExample.Data;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers
{
    [Route("api/[controller]")]
    public class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(AppDbContext context) : base(context)
        { }
    }
}
