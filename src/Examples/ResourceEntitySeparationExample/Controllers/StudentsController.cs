using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models.Resources;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class StudentsController : JsonApiController<StudentResource>
    {
        public StudentsController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<StudentResource, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
