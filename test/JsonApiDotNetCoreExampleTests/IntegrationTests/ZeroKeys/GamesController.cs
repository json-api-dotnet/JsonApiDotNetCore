using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ZeroKeys
{
    public sealed class GamesController : JsonApiController<Game, int?>
    {
        public GamesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Game, int?> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
