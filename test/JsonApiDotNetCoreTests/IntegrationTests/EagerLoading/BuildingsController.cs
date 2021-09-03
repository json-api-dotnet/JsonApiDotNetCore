using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    public sealed class BuildingsController : JsonApiController<Building>
    {
        public BuildingsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Building> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
