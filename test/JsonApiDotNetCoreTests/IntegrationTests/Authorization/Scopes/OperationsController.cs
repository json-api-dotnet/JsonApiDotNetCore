using System.Net;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

public sealed class OperationsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IOperationsProcessor processor, IJsonApiRequest request,
    ITargetedFields targetedFields, IAtomicOperationFilter operationFilter) : JsonApiOperationsController(options, resourceGraph, loggerFactory, processor,
    request, targetedFields, operationFilter)
{
    public override async Task<IActionResult> PostOperationsAsync(IList<OperationContainer> operations, CancellationToken cancellationToken)
    {
        AuthScopeSet requestedScopes = AuthScopeSet.GetRequestedScopes(HttpContext.Request.Headers);
        AuthScopeSet requiredScopes = GetRequiredScopes(operations);

        if (!requestedScopes.ContainsAll(requiredScopes))
        {
            return Error(new ErrorObject(HttpStatusCode.Unauthorized)
            {
                Title = "Insufficient permissions to perform this request.",
                Detail = $"Performing this request requires the following scopes: {requiredScopes}.",
                Source = new ErrorSource
                {
                    Header = AuthScopeSet.ScopesHeaderName
                }
            });
        }

        return await base.PostOperationsAsync(operations, cancellationToken);
    }

    private AuthScopeSet GetRequiredScopes(IEnumerable<OperationContainer> operations)
    {
        var requiredScopes = new AuthScopeSet();

        foreach (OperationContainer operation in operations)
        {
            requiredScopes.IncludeFrom(operation.Request, operation.TargetedFields);
        }

        return requiredScopes;
    }
}
