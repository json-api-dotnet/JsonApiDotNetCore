using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models.Resources;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class StudentsController : JsonApiController<StudentResource>
    {
        public StudentsController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<StudentResource, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
