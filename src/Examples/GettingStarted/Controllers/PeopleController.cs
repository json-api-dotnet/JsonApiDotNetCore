using GettingStarted.Models;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

namespace GettingStarted
{
    public class PeopleController : JsonApiController<Person>
    {
        public PeopleController(
          IJsonApiContext jsonApiContext,
          IResourceService<Person> resourceService)
          : base(jsonApiContext, resourceService)
        { }
    }
}