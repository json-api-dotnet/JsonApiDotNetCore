using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    public sealed class CalendarsController : JsonApiController<Calendar, int>
    {
        public CalendarsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Calendar> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
