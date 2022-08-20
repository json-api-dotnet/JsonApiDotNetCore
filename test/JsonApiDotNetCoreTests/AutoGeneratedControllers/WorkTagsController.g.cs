using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

public sealed partial class WorkTagsController : JsonApiController<WorkTag, int>
{
    public WorkTagsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<WorkTag, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
