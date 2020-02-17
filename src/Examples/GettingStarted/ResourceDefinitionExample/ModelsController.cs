using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace GettingStarted.ResourceDefinitionExample
{
    public sealed class ModelsController : JsonApiController<Model>
    {
        public ModelsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Model> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
