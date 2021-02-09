using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class PassportsController : JsonApiController<Passport>
    {
        public PassportsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Passport> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
