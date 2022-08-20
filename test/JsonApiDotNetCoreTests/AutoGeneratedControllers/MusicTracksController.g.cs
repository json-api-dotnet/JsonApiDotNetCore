using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

public sealed partial class MusicTracksController : JsonApiController<MusicTrack, System.Guid>
{
    public MusicTracksController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<MusicTrack, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
