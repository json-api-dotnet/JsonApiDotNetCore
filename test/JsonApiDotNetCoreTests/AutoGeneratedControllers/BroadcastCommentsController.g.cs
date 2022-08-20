using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Archiving;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

public sealed partial class BroadcastCommentsController : JsonApiController<BroadcastComment, int>
{
    public BroadcastCommentsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<BroadcastComment, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
