using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class EnginesController : JsonApiController<Engine>
    {
        public EnginesController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Engine> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
