using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceHooks.Controllers
{
    public sealed class PassportsController : JsonApiController<Passport>
    {
        public PassportsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Passport> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
