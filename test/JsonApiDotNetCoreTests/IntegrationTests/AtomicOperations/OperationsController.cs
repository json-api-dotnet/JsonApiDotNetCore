#nullable disable

using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    public sealed class OperationsController : JsonApiOperationsController
    {
        public OperationsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IOperationsProcessor processor, IJsonApiRequest request,
            ITargetedFields targetedFields)
            : base(options, resourceGraph, loggerFactory, processor, request, targetedFields)
        {
        }
    }
}
