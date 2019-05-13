using System;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TodoCollectionsController : JsonApiController<TodoItemCollection, Guid>
    {
        readonly IDbContextResolver _dbResolver;

        public TodoCollectionsController(
           IDbContextResolver contextResolver,
           IJsonApiContext jsonApiContext,
           IResourceService<TodoItemCollection, Guid> resourceService,
           ILoggerFactory loggerFactory)
           : base(jsonApiContext, resourceService, loggerFactory)
        {
            _dbResolver = contextResolver;
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(Guid id, [FromBody] TodoItemCollection entity)
        {
            if (entity.Name == "PRE-ATTACH-TEST")
            {
                var targetTodoId = entity.TodoItems.First().Id;
                var todoItemContext = _dbResolver.GetDbSet<TodoItem>();
                await todoItemContext.Where(ti => ti.Id == targetTodoId).FirstOrDefaultAsync();
            }
            return await base.PatchAsync(id, entity);
        }

    }
}
