using GettingStarted.Models;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace GettingStarted
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