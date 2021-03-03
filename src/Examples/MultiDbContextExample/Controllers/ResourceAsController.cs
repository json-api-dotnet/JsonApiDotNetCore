using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using MultiDbContextExample.Models;

namespace MultiDbContextExample.Controllers
{
    public sealed class ResourceAsController : JsonApiController<ResourceA>
    {
        public ResourceAsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ResourceA> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
