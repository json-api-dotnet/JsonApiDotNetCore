using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    public sealed class EngagementsController : JsonApiController<Engagement, Guid>
    {
        public EngagementsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Engagement, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
