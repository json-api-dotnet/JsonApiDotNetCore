using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests
{
    public sealed class AirplanesController : JsonApiController<Airplane, int>
    {
        public AirplanesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Airplane, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
