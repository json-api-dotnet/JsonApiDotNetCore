using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PersonRolesController : JsonApiController<PersonRole>
    {
        public PersonRolesController(
            IJsonApiOptions jsonApiOptions,
            IResourceGraph resourceGraph,
            IResourceService<PersonRole> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, resourceGraph, resourceService, loggerFactory)
        { }
    }
}
