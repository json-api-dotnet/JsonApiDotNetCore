using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Controllers
{
    public sealed class TodoItemsController : JsonApiController<TodoItem>
    {
        public TodoItemsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<TodoItem> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
