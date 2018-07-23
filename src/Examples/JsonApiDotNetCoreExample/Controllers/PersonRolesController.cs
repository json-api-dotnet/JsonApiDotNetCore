using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PersonRolesController : JsonApiController<PersonRole>
    {
        public PersonRolesController(
            IJsonApiContext jsonApiContext,
            IResourceService<PersonRole> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
