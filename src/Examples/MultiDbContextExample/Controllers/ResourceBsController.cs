using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using MultiDbContextExample.Models;

namespace MultiDbContextExample.Controllers
{
    public sealed class ResourceBsController : JsonApiController<ResourceB, int>
    {
        public ResourceBsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ResourceB, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
