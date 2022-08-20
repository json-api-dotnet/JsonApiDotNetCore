using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Serialization;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

public sealed partial class MeetingAttendeesController : JsonApiController<MeetingAttendee, System.Guid>
{
    public MeetingAttendeesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<MeetingAttendee, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
