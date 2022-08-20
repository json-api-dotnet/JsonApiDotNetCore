using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

public sealed partial class LyricsController : JsonApiController<Lyric, long>
{
    public LyricsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Lyric, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
