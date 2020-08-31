using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class ThrowingResourcesController : JsonApiController<ThrowingResource>
    {
        public ThrowingResourcesController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<ThrowingResource> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
