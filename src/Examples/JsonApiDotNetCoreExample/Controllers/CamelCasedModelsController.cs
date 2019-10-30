using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class CamelCasedModelsController : JsonApiController<CamelCasedModel>
    {
        public CamelCasedModelsController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<CamelCasedModel> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceService, loggerFactory)
        { }
    }
}
