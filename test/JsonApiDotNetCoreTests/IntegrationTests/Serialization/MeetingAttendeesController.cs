using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Serialization
{
    public sealed class MeetingAttendeesController : JsonApiController<MeetingAttendee, Guid>
    {
        public MeetingAttendeesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<MeetingAttendee, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
