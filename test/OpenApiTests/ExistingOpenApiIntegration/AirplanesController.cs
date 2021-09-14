using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.ExistingOpenApiIntegration
{
    public sealed class AirplanesController : JsonApiController<Airplane, string>
    {
        public AirplanesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Airplane, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
