using GettingStarted.Models;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace GettingStarted.ResourceDefinitionExample
{
    public class ModelsController : JsonApiController<Model>
    {
        public ModelsController(
          IJsonApiContext jsonApiContext,
          IResourceService<Model> resourceService)
          : base(jsonApiContext, resourceService)
        { }
    }
}