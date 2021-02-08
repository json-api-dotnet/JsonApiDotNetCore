using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Serialization
{
    public sealed class MeetingsController : JsonApiController<Meeting, Guid>
    {
        public MeetingsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Meeting, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
