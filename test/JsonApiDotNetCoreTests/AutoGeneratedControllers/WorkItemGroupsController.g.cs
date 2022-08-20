using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

public sealed partial class WorkItemGroupsController : JsonApiController<WorkItemGroup, System.Guid>
{
    public WorkItemGroupsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<WorkItemGroup, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
