using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PeopleController : JsonApiController<Person>
    {
        public PeopleController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceService<Person> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        {
        }
    }
}
