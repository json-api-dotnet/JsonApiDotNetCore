using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

public sealed partial class PlaylistsController : JsonApiController<Playlist, long>
{
    public PlaylistsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Playlist, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
