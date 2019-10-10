using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PassportsController : JsonApiController<Passport>
    {
        public PassportsController(IJsonApiOptions jsonApiOptions, IResourceGraph resourceGraph, IResourceService<Passport, int> resourceService, ILoggerFactory loggerFactory = null) : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        {
        }
    }
}
