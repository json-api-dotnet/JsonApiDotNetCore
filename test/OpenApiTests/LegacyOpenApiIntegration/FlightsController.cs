using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    public sealed class FlightsController : JsonApiController<Flight, string>
    {
        public FlightsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Flight, string> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
