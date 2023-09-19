using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCoreTests.IntegrationTests.Authorization.Scopes;

// Implements IActionFilter instead of IAuthorizationFilter because it needs to execute *after* parsing query string parameters.
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class ScopesAuthorizationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.RequestServices.GetRequiredService<IJsonApiRequest>();
        var targetedFields = context.HttpContext.RequestServices.GetRequiredService<ITargetedFields>();
        var constraintProviders = context.HttpContext.RequestServices.GetRequiredService<IEnumerable<IQueryConstraintProvider>>();

        if (request.Kind == EndpointKind.AtomicOperations)
        {
            // Handled in operators controller, because it requires access to the individual operations.
            return;
        }

        AuthScopeSet requestedScopes = AuthScopeSet.GetRequestedScopes(context.HttpContext.Request.Headers);
        AuthScopeSet requiredScopes = GetRequiredScopes(request, targetedFields, constraintProviders);

        if (!requestedScopes.ContainsAll(requiredScopes))
        {
            context.Result = new UnauthorizedObjectResult(new ErrorObject(HttpStatusCode.Unauthorized)
            {
                Title = "Insufficient permissions to perform this request.",
                Detail = $"Performing this request requires the following scopes: {requiredScopes}.",
                Source = new ErrorSource
                {
                    Header = AuthScopeSet.ScopesHeaderName
                }
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private AuthScopeSet GetRequiredScopes(IJsonApiRequest request, ITargetedFields targetedFields, IEnumerable<IQueryConstraintProvider> constraintProviders)
    {
        var requiredScopes = new AuthScopeSet();
        requiredScopes.IncludeFrom(request, targetedFields);

        var walker = new QueryStringWalker(requiredScopes);
        walker.IncludeScopesFrom(constraintProviders);

        return requiredScopes;
    }

    private sealed class QueryStringWalker : QueryExpressionRewriter<object?>
    {
        private readonly AuthScopeSet _authScopeSet;

        public QueryStringWalker(AuthScopeSet authScopeSet)
        {
            _authScopeSet = authScopeSet;
        }

        public void IncludeScopesFrom(IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            foreach (ExpressionInScope constraint in constraintProviders.SelectMany(provider => provider.GetConstraints()))
            {
                Visit(constraint.Expression, null);
            }
        }

        public override QueryExpression VisitIncludeElement(IncludeElementExpression expression, object? argument)
        {
            _authScopeSet.Include(expression.Relationship, Permission.Read);

            return base.VisitIncludeElement(expression, argument);
        }

        public override QueryExpression VisitResourceFieldChain(ResourceFieldChainExpression expression, object? argument)
        {
            foreach (ResourceFieldAttribute field in expression.Fields)
            {
                if (field is RelationshipAttribute relationship)
                {
                    _authScopeSet.Include(relationship, Permission.Read);
                }
                else
                {
                    _authScopeSet.Include(field.Type, Permission.Read);
                }
            }

            return base.VisitResourceFieldChain(expression, argument);
        }
    }
}
