using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    public sealed class AirplanesController : JsonApiController<Airplane, string>
    {
        public AirplanesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Airplane, string> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
