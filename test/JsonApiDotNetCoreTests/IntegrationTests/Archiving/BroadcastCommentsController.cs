using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Archiving
{
    public sealed class BroadcastCommentsController : JsonApiController<BroadcastComment>
    {
        public BroadcastCommentsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<BroadcastComment> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
