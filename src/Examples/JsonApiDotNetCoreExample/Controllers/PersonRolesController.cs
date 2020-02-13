using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PersonRolesController : JsonApiController<PersonRole>
    {
        public PersonRolesController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<PersonRole> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
