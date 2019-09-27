using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;

namespace GettingStarted.ResourceDefinitionExample
{
    public class ModelsController : JsonApiController<Model>
    {
        public ModelsController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<Model> resourceService)
          : base(jsonApiOptions, resourceGraph, resourceService)
        { }
    }
}
