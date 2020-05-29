using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.RequestServices.Contracts;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <summary>
    /// Reads custom query string parameters for which handlers on <see cref="ResourceDefinition{TResource}"/> are registered
    /// and produces a set of query constraints from it.
    /// </summary>
    public interface IResourceDefinitionQueryableParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }

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

        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return true;
        }

        public bool CanRead(string parameterName)
        {
            var queryableHandler = GetQueryableHandler(parameterName);
            return queryableHandler != null;
        }

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

        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _constraints.AsReadOnly();
        }
    }
}
