using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys;

public sealed partial class PlayersController : JsonApiController<Player, string>
{
    public PlayersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Player, string> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
