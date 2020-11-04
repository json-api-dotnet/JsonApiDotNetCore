using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CompositeKeys
{
    public sealed class JourneysController : JsonApiController<Journey>
    {
        public JourneysController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Journey> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
