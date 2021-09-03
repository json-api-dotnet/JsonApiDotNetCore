using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices
{
    public sealed class DomainGroupsController : JsonApiController<DomainGroup, Guid>
    {
        public DomainGroupsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<DomainGroup, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
