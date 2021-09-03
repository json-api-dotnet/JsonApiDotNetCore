using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    public sealed class PlanetsController : JsonApiController<Planet>
    {
        public PlanetsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Planet> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
