using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    public sealed class SwimmingPoolsController : JsonApiController<SwimmingPool>
    {
        public SwimmingPoolsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SwimmingPool> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
