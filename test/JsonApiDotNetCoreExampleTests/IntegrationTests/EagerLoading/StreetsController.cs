using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.EagerLoading
{
    public sealed class StreetsController : JsonApiController<Street>
    {
        public StreetsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Street> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
