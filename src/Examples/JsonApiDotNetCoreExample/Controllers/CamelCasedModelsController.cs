using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [Route("[controller]")]
    [DisableRoutingConvention]
    public class CamelCasedModelsController : JsonApiController<CamelCasedModel>
    {
        public CamelCasedModelsController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<CamelCasedModel> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
