using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class ChampionshipsController : JsonApiController<Championship, Guid>
    {
        public ChampionshipsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Championship, Guid> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}

