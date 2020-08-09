using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExample.Controllers
{
    public sealed class TeamsController : JsonApiController<Team, Guid>
    {
        public TeamsController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<Team, Guid> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }
    }
}
