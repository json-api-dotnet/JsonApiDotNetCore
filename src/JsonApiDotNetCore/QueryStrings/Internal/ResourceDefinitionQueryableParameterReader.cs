using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
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
        private readonly IResourceDefinitionProvider _resourceDefinitionProvider;
        private readonly List<ExpressionInScope> _constraints = new List<ExpressionInScope>();

        public ResourceDefinitionQueryableParameterReader(IJsonApiRequest request, IResourceDefinitionProvider resourceDefinitionProvider)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _resourceDefinitionProvider = resourceDefinitionProvider ?? throw new ArgumentNullException(nameof(resourceDefinitionProvider));
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            return true;
        }

        /// <inheritdoc />
        public virtual bool CanRead(string parameterName)
        {
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
            if (_request.Kind != EndpointKind.Primary)
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "Custom query string parameters cannot be used on nested resource endpoints.",
                    $"Query string parameter '{parameterName}' cannot be used on a nested resource endpoint.");
            }

            var resourceType = _request.PrimaryResource.ResourceType;
            var resourceDefinition = _resourceDefinitionProvider.Get(resourceType);
            return resourceDefinition?.GetQueryableHandlerForQueryStringParameter(parameterName);
        }

        /// <inheritdoc />
        public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _constraints;
        }
    }
}
