using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions
{
    public sealed class SwimmingPoolsController : JsonApiController<SwimmingPool, int>
    {
        public SwimmingPoolsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SwimmingPool, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
