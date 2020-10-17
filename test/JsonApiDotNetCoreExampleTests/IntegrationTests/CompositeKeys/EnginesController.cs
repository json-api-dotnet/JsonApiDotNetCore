using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class EnginesController : JsonApiController<Engine>
    {
        public EnginesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Engine> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
