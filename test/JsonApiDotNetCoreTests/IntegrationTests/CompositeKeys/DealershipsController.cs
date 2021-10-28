using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys
{
    public sealed class DealershipsController : JsonApiController<Dealership, int>
    {
        public DealershipsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Dealership, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
