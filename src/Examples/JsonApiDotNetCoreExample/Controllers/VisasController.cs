using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class VisasController : JsonApiController<Visa>
    {
        public VisasController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Visa> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
