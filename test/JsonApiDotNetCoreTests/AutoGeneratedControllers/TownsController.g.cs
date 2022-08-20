using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

namespace JsonApiDotNetCoreTests.IntegrationTests.CustomRoutes;

public sealed partial class TownsController : JsonApiController<Town, int>
{
    public TownsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Town, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
