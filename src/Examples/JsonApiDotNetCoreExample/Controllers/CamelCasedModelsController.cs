using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class CamelCasedModelsController : JsonApiController<CamelCasedModel>
    {
        public CamelCasedModelsController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<CamelCasedModel> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
