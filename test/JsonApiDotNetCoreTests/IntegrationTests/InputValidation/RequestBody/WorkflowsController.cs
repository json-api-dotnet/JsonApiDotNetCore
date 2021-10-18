#nullable disable

using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody
{
    public sealed class WorkflowsController : JsonApiController<Workflow, Guid>
    {
        public WorkflowsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Workflow, Guid> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
