using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    public sealed class TelevisionNetworksController : JsonApiController<TelevisionNetwork, int>
    {
        public TelevisionNetworksController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<TelevisionNetwork> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
