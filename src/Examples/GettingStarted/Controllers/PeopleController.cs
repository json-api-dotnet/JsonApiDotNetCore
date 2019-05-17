using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace GettingStarted
{
    public class PeopleController : JsonApiController<Person>
    {
        public PeopleController(
            IJsonApiOptions jsonApiOptions, 
            IJsonApiContext jsonApiContext,
            IResourceService<Person> resourceService)
          : base(jsonApiOptions,jsonApiContext, resourceService)
        { }
    }
}
