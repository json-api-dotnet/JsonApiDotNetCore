using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

public sealed partial class CalendarsController : JsonApiController<Calendar, int>
{
    public CalendarsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Calendar, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
