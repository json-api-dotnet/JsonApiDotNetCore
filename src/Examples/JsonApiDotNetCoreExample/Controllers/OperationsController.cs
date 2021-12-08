using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExample.Controllers;

public sealed class OperationsController : JsonApiOperationsController
{
    public OperationsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IOperationsProcessor processor,
        IJsonApiRequest request, ITargetedFields targetedFields)
        : base(options, resourceGraph, loggerFactory, processor, request, targetedFields)
    {
    }
}
