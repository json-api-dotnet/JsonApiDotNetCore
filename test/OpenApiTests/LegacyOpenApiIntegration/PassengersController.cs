using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.LegacyOpenApiIntegration
{
    public sealed class PassengersController : JsonApiController<Passenger, string>
    {
        public PassengersController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Passenger, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
