using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading;

public sealed partial class StatesController : JsonApiController<State, int>
{
    public StatesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<State, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
