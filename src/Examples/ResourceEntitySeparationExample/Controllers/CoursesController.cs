using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models.Resources;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class CoursesController : JsonApiController<CourseResource>
    {
        public CoursesController(
            IJsonApiContext jsonApiContext,
            IResourceService<CourseResource> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
