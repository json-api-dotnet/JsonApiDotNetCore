using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class KebabCasedModelsController : JsonApiController<KebabCasedModel>
    {
        public KebabCasedModelsController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<KebabCasedModel> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceService, loggerFactory)
        { }
    }
}
