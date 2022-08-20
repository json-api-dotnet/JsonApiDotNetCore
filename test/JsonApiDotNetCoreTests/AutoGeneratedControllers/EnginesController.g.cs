using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

public sealed partial class EnginesController : JsonApiController<Engine, int>
{
    public EnginesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Engine, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
