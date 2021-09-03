using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Reading
{
    public sealed class MoonsController : JsonApiController<Moon>
    {
        public MoonsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Moon> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
