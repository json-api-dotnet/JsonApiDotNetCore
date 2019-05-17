using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models.Resources;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class CoursesController : JsonApiController<CourseResource>
    {
        public CoursesController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<CourseResource, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
