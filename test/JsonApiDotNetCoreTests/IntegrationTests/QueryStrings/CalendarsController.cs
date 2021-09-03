using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class CalendarsController : JsonApiController<Calendar>
    {
        public CalendarsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Calendar> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
