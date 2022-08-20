using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

public sealed partial class MapsController : JsonApiController<Map, System.Guid?>
{
    public MapsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Map, System.Guid?> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
