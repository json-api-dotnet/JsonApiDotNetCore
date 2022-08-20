using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Archiving;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

public sealed partial class TelevisionBroadcastsController : JsonApiController<TelevisionBroadcast, int>
{
    public TelevisionBroadcastsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<TelevisionBroadcast, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
