using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests
{
    public sealed class AirplanesController : JsonApiController<Airplane>
    {
        public AirplanesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Airplane> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
