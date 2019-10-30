using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PassportsController : JsonApiController<Passport>
    {
        public PassportsController(IJsonApiOptions jsonApiOptions,
                                   IResourceService<Passport, int> resourceService,
                                   ILoggerFactory loggerFactory = null)
            : base(jsonApiOptions, resourceService, loggerFactory)
        {
        }
    }
}
