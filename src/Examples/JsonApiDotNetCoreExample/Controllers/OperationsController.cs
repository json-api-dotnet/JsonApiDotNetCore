using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCoreExample.DocAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers;

public sealed class OperationsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IOperationsProcessor processor, IJsonApiRequest request,
    ITargetedFields targetedFields, IAtomicOperationFilter operationFilter)
    : JsonApiOperationsController(options, resourceGraph, loggerFactory, processor, request, targetedFields, operationFilter)
{
    [HttpPost]
    [RequiresAdmin]
    public override Task<IActionResult> PostOperationsAsync([Required] IList<OperationContainer> operations, CancellationToken cancellationToken)
    {
        return base.PostOperationsAsync(operations, cancellationToken);
    }
}
