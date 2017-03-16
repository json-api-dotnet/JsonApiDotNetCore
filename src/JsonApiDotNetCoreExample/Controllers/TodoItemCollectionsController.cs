using System;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TodoItemCollectionsController : JsonApiController<TodoItemCollection, Guid>
    {
         public TodoItemCollectionsController(
            IJsonApiContext jsonApiContext,
            IEntityRepository<TodoItemCollection, Guid> entityRepository,
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, entityRepository, loggerFactory)
        { }
    }
}