using GettingStarted.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Services;

namespace GettingStarted
{
    public class PeopleController : JsonApiController<Person>
    {
        public PeopleController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<Person> resourceService)
          : base(jsonApiOptions, resourceService)
        { }
    }
}
