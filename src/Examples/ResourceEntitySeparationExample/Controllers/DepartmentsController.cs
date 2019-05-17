using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models.Resources;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class DepartmentsController : JsonApiController<DepartmentResource>
    {
        public DepartmentsController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<DepartmentResource, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
