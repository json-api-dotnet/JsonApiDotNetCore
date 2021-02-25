using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class PeopleController : JsonApiController<Person>
    {
        public PeopleController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Person> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
