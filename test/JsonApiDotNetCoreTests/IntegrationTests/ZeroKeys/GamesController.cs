#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys
{
    public sealed class GamesController : JsonApiController<Game, int?>
    {
        public GamesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Game, int?> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
