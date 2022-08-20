using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

public sealed partial class CiviliansController : JsonApiController<Civilian, int>
{
    public CiviliansController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Civilian, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
