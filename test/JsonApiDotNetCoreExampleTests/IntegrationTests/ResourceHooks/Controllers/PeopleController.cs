using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Controllers
{
    public sealed class PeopleController : JsonApiController<Person>
    {
        public PeopleController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Person> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
