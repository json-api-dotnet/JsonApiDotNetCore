using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    public sealed class FlightAttendantsController : JsonApiController<FlightAttendant, long>
    {
        public FlightAttendantsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<FlightAttendant, long> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
