using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public class PeopleController : JsonApiController<Person>
    {
        public PeopleController(
            IJsonApiContext jsonApiContext,
            IResourceService<Person> resourceService,
            ILoggerFactory loggerFactory) 
            : base(jsonApiContext, resourceService, loggerFactory)
        { }
    }
}
