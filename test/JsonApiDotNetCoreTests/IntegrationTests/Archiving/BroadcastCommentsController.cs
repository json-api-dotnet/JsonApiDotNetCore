using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    public sealed class BroadcastCommentsController : JsonApiController<BroadcastComment, int>
    {
        public BroadcastCommentsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<BroadcastComment, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
