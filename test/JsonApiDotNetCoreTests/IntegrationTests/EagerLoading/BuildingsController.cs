using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    public sealed class BuildingsController : JsonApiController<Building, int>
    {
        public BuildingsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Building, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
