using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Parsing;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.RequestServices.Contracts;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    public class SortQueryStringParameterReader : QueryStringParameterReader, ISortQueryStringParameterReader
    {
        private readonly QueryStringParameterScopeParser _scopeParser;
        private readonly SortParser _sortParser;
        private readonly List<ExpressionInScope> _constraints = new List<ExpressionInScope>();
        private string _lastParameterName;

        public SortQueryStringParameterReader(ICurrentRequest currentRequest, IResourceContextProvider resourceContextProvider)
            : base(currentRequest, resourceContextProvider)
        {
            _scopeParser = new QueryStringParameterScopeParser(resourceContextProvider, FieldChainRequirements.EndsInToMany);
            _sortParser = new SortParser(resourceContextProvider, ValidateSingleField);
        }

        private void ValidateSingleField(ResourceFieldAttribute field, ResourceContext resourceContext, string path)
        {
            if (field is AttrAttribute attribute && !attribute.Capabilities.HasFlag(AttrCapabilities.AllowSort))
            {
                throw new InvalidQueryStringParameterException(_lastParameterName, "Sorting on the requested attribute is not allowed.",
                    $"Sorting on attribute '{attribute.PublicName}' is not allowed.");
            }
        }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Sort);
        }

        /// <inheritdoc/>
        public bool CanRead(string parameterName)
        {
            var isNested = parameterName.StartsWith("sort[") && parameterName.EndsWith("]");
            return parameterName == "sort" || isNested;
        }

        /// <inheritdoc/>
        public void Read(string parameterName, StringValues parameterValue)
        {
            _lastParameterName = parameterName;

            try
            {
                ResourceFieldChainExpression scope = GetScope(parameterName);
                SortExpression sort = GetSort(parameterValue, scope);

                var expressionInScope = new ExpressionInScope(scope, sort);
                _constraints.Add(expressionInScope);
            }
            catch (QueryParseException exception)
            {
                throw new InvalidQueryStringParameterException(parameterName, "The specified sort is invalid.", exception.Message, exception);
            }
        }

        private ResourceFieldChainExpression GetScope(string parameterName)
        {
            var parameterScope = _scopeParser.Parse(parameterName, RequestResource);

            if (parameterScope.Scope == null)
            {
                AssertIsCollectionRequest();
            }

            return parameterScope.Scope;
        }

        private SortExpression GetSort(string parameterValue, ResourceFieldChainExpression scope)
        {
            ResourceContext resourceContextInScope = GetResourceContextForScope(scope);
            return _sortParser.Parse(parameterValue, resourceContextInScope);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _constraints;
        }
    }
}
