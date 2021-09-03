using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class LyricsController : JsonApiController<Lyric, long>
    {
        public LyricsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Lyric, long> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
