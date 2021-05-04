using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class PerformersController : JsonApiController<Performer>
    {
        public PerformersController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Performer> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
