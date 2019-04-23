using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class TagsController : JsonApiController<Tag>
    {
        public TagsController(
            IJsonApiContext jsonApiContext,
            IResourceService<Tag> resourceService)
            : base(jsonApiContext, resourceService)
        { }
    }
}
