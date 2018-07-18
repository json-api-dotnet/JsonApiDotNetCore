using System;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models.Resources;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class DepartmentsController : JsonApiController<DepartmentResource>
    {
        public DepartmentsController(
            IJsonApiContext jsonApiContext,
            IResourceService<DepartmentResource> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
