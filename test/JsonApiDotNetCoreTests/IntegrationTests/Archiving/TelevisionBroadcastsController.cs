#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    public sealed class TelevisionBroadcastsController : JsonApiController<TelevisionBroadcast, int>
    {
        public TelevisionBroadcastsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<TelevisionBroadcast, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
