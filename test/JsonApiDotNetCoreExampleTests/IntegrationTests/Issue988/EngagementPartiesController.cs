using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    public sealed class EngagementPartiesController : JsonApiController<EngagementParty, Guid>
    {
        public EngagementPartiesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<EngagementParty, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
