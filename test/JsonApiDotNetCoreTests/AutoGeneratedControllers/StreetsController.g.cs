using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

public sealed partial class StreetsController : JsonApiController<Street, int>
{
    public StreetsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Street, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
