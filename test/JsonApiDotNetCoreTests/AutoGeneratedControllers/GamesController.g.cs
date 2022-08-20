using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

public sealed partial class GamesController : JsonApiController<Game, int?>
{
    public GamesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Game, int?> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
