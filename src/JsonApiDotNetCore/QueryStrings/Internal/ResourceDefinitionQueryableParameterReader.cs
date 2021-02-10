using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    /// <inheritdoc />
    public class ResourceDefinitionQueryableParameterReader : IResourceDefinitionQueryableParameterReader
    {
        private readonly IJsonApiRequest _request;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly List<ExpressionInScope> _constraints = new List<ExpressionInScope>();

        public ResourceDefinitionQueryableParameterReader(IJsonApiRequest request, IResourceDefinitionAccessor resourceDefinitionAccessor)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _resourceDefinitionAccessor = resourceDefinitionAccessor ?? throw new ArgumentNullException(nameof(resourceDefinitionAccessor));
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual bool CanRead(string parameterName)
        {
            if (_request.Kind == EndpointKind.AtomicOperations)
            {
                return false;
            }

            var queryableHandler = GetQueryableHandler(parameterName);
            return queryableHandler != null;
        }

        /// <inheritdoc />
        public virtual void Read(string parameterName, StringValues parameterValue)
        {
            var queryableHandler = GetQueryableHandler(parameterName);
            var expressionInScope = new ExpressionInScope(null, new QueryableHandlerExpression(queryableHandler, parameterValue));
            _constraints.Add(expressionInScope);
        }

        private object GetQueryableHandler(string parameterName)
        {
            var resourceType = _request.PrimaryResource.ResourceType;
            var handler = _resourceDefinitionAccessor.GetQueryableHandlerForQueryStringParameter(resourceType, parameterName);

            if (handler != null && _request.Kind != EndpointKind.Primary)
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "Custom query string parameters cannot be used on nested resource endpoints.",
                    $"Query string parameter '{parameterName}' cannot be used on a nested resource endpoint.");
            }

            return handler;
        }

        /// <inheritdoc />
        public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _constraints;
        }
    }
}
