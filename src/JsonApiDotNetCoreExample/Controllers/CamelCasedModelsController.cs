using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    [DisableRoutingConvention]
    public class CamelCasedModelsController : JsonApiController<CamelCasedModel>
    {
        public CamelCasedModelsController(
            IJsonApiContext jsonApiContext,
            IResourceService<CamelCasedModel> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
