using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace GettingStarted.ResourceDefinitionExample
{
    public class ModelsController : JsonApiController<Model>
    {
        public ModelsController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<Model> resourceService)
          : base(jsonApiOptions, resourceService)
        { }
    }
}
