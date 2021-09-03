using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    public sealed class PlaylistsController : JsonApiController<Playlist, long>
    {
        public PlaylistsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Playlist, long> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
