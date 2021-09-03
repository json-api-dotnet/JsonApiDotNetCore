using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading
{
    public sealed class StarsController : JsonApiController<Star>
    {
        public StarsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Star> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
