using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys
{
    public sealed class EnginesController : JsonApiController<Engine, int>
    {
        public EnginesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Engine> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
