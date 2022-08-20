using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody;

public sealed partial class WorkflowsController : JsonApiController<Workflow, System.Guid>
{
    public WorkflowsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Workflow, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
