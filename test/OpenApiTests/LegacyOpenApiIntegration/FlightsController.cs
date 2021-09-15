using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    public sealed class FlightsController : JsonApiController<Flight>
    {
        public FlightsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Flight> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
