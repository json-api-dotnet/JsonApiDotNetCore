using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class PlayersController : JsonApiController<Player, Guid>
    {
        public PlayersController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Player, Guid> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
