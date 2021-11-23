using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    public sealed class FlightAttendantsController : JsonApiController<FlightAttendant, string>
    {
        public FlightAttendantsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<FlightAttendant, string> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
