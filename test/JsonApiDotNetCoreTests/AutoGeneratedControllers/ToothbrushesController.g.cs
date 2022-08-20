using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults;

namespace JsonApiDotNetCoreTests.IntegrationTests.ControllerActionResults;

public sealed partial class ToothbrushesController : JsonApiController<Toothbrush, int>
{
    public ToothbrushesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Toothbrush, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
