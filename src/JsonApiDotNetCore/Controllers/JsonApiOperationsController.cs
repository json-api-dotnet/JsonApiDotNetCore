using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers;

/// <summary>
/// The base class to derive atomic:operations controllers from. This class delegates all work to <see cref="BaseJsonApiOperationsController" /> but adds
/// attributes for routing templates. If you want to provide routing templates yourself, you should derive from BaseJsonApiOperationsController directly.
/// </summary>
public abstract class JsonApiOperationsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IOperationsProcessor processor, IJsonApiRequest request,
    ITargetedFields targetedFields, IAtomicOperationFilter operationFilter)
    : BaseJsonApiOperationsController(options, resourceGraph, loggerFactory, processor, request, targetedFields, operationFilter)
{
    /// <inheritdoc />
    [HttpPost]
    public override async Task<IActionResult> PostOperationsAsync([Required] IList<OperationContainer> operations, CancellationToken cancellationToken)
    {
        return await base.PostOperationsAsync(operations, cancellationToken);
    }
}
