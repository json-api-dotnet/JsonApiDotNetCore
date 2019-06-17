using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PassportsController : JsonApiController<Passport>
    {
        public PassportsController(
            IJsonApiContext jsonApiContext,
            IResourceService<Passport> resourceService) 
            : base(jsonApiContext, resourceService)
        { }
    }
}