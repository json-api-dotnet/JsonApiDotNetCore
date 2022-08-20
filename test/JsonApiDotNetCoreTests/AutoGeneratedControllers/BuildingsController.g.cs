using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

public sealed partial class BuildingsController : JsonApiController<Building, int>
{
    public BuildingsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Building, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
