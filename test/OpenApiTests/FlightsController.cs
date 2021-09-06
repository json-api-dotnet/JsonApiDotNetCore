using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests
{
    public sealed class FlightsController : JsonApiController<Flight, int>
    {
        public FlightsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Flight, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
