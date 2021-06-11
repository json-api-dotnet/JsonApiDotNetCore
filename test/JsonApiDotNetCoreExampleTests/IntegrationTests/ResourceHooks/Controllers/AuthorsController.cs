using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Controllers
{
    public sealed class AuthorsController : JsonApiController<Author>
    {
        public AuthorsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Author> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
