using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace GettingStarted
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
