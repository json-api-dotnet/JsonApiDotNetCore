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
        public WorkflowsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Workflow, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
