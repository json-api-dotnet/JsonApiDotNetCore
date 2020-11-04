using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class DealershipsController : JsonApiController<Dealership>
    {
        public DealershipsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Dealership> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
