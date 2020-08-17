using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.RequestServices.Contracts;
using JsonApiDotNetCore.Services.Contract;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <inheritdoc/>
    public class ResourceDefinitionQueryableParameterReader : IResourceDefinitionQueryableParameterReader
    {
        private readonly ICurrentRequest _currentRequest;
        private readonly IResourceDefinitionProvider _resourceDefinitionProvider;
        private readonly List<ExpressionInScope> _constraints = new List<ExpressionInScope>();

        public ResourceDefinitionQueryableParameterReader(ICurrentRequest currentRequest, IResourceDefinitionProvider resourceDefinitionProvider)
        {
            _currentRequest = currentRequest;
            _resourceDefinitionProvider = resourceDefinitionProvider;
        }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool CanRead(string parameterName)
        {
            var queryableHandler = GetQueryableHandler(parameterName);
            return queryableHandler != null;
        }

        /// <inheritdoc/>
        public void Read(string parameterName, StringValues parameterValue)
        {
            var queryableHandler = GetQueryableHandler(parameterName);
            var expressionInScope = new ExpressionInScope(null, new QueryableHandlerExpression(queryableHandler, parameterValue));
            _constraints.Add(expressionInScope);
        }

        private object GetQueryableHandler(string parameterName)
        {
            if (_currentRequest.Kind != EndpointKind.Primary)
            {
                throw new InvalidQueryStringParameterException(parameterName,
                    "Custom query string parameters cannot be used on nested resource endpoints.",
                    $"Query string parameter '{parameterName}' cannot be used on a nested resource endpoint.");
            }

            var resourceType = _currentRequest.PrimaryResource.ResourceType;
            var resourceDefinition = _resourceDefinitionProvider.Get(resourceType);
            return resourceDefinition?.GetQueryableHandlerForQueryStringParameter(parameterName);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _constraints.AsReadOnly();
        }
    }
}
