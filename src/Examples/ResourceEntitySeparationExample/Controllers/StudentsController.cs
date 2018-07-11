using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class StudentsController : JsonApiController<StudentDto>
    {
        public StudentsController(
            IJsonApiContext jsonApiContext,
            IResourceService<StudentDto> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
