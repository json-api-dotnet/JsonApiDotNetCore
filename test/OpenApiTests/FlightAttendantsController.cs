using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests
{
    public sealed class FlightAttendantsController : JsonApiController<FlightAttendant, string>
    {
        public FlightAttendantsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<FlightAttendant, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
