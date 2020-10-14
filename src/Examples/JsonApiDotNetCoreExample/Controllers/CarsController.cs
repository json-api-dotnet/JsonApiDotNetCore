using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class CarsController : JsonApiController<Car, string>
    {
        public CarsController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<Car, string> resourceService)
            : base(options, loggerFactory, resourceService)
        { }
    }
}
