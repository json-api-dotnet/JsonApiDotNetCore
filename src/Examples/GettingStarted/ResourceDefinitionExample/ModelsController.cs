using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace GettingStarted.ResourceDefinitionExample
{
    public class ModelsController : JsonApiController<Model>
    {
        public ModelsController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<Model> resourceService)
          : base(jsonApiOptions, jsonApiContext, resourceService)
        { }
    }
}
