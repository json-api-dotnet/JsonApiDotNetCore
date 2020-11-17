using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class TodoCollectionsController : JsonApiController<TodoItemCollection, Guid>
    {
        private readonly IDbContextResolver _dbResolver;

        public TodoCollectionsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IDbContextResolver contextResolver,
            IResourceService<TodoItemCollection, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
            _dbResolver = contextResolver;
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(Guid id, [FromBody] TodoItemCollection resource, CancellationToken cancellationToken)
        {
            if (resource.Name == "PRE-ATTACH-TEST")
            {
                var targetTodoId = resource.TodoItems.First().Id;
                var todoItemContext = _dbResolver.GetContext().Set<TodoItem>();
                await todoItemContext.Where(ti => ti.Id == targetTodoId).FirstOrDefaultAsync(cancellationToken);
            }

            return await base.PatchAsync(id, resource, cancellationToken);
        }

    }
}
