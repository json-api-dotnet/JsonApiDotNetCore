using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class PlayersController : JsonApiController<Player, string>
    {
        public PlayersController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Player, string> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
