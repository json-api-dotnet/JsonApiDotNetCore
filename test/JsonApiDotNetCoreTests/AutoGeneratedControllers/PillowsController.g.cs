using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed partial class PillowsController : JsonApiController<Pillow, int>
{
    public PillowsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Pillow, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
