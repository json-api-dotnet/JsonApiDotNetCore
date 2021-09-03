using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    public sealed class SupportTicketsController : JsonApiController<SupportTicket>
    {
        public SupportTicketsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SupportTicket> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
