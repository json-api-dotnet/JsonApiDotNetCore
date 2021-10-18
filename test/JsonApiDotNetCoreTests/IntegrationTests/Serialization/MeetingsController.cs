#nullable disable

using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization
{
    public sealed class MeetingsController : JsonApiController<Meeting, Guid>
    {
        public MeetingsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Meeting, Guid> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
