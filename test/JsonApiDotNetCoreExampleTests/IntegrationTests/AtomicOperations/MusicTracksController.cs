using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class MusicTracksController : JsonApiController<MusicTrack, Guid>
    {
        public MusicTracksController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<MusicTrack, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
