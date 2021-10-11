#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    public sealed class SupportTicketsController : JsonApiController<SupportTicket, int>
    {
        public SupportTicketsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SupportTicket, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
