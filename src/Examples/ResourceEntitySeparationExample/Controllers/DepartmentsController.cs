using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models.Resources;
using Microsoft.Extensions.Logging;

namespace ResourceEntitySeparationExample.Controllers
{
    public class DepartmentsController : JsonApiController<DepartmentResource>
    {
        public DepartmentsController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<DepartmentResource, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
