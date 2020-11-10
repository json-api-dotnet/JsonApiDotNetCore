using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    public sealed class WorkItemGroupsController : JsonApiController<WorkItemGroup, Guid>
    {
        public WorkItemGroupsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<WorkItemGroup, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
