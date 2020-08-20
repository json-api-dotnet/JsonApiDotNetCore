using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    public class SparseFieldSetQueryStringParameterReader : QueryStringParameterReader, ISparseFieldSetQueryStringParameterReader
    {
        private readonly QueryStringParameterScopeParser _scopeParser;
        private readonly SparseFieldSetParser _sparseFieldSetParser;
        private readonly List<ExpressionInScope> _constraints = new List<ExpressionInScope>();
        private string _lastParameterName;

        public SparseFieldSetQueryStringParameterReader(IJsonApiRequest request, IResourceContextProvider resourceContextProvider)
            : base(request, resourceContextProvider)
        {
            _sparseFieldSetParser = new SparseFieldSetParser(resourceContextProvider, ValidateSingleAttribute);
            _scopeParser = new QueryStringParameterScopeParser(resourceContextProvider, FieldChainRequirements.IsRelationship);
        }

        private void ValidateSingleAttribute(AttrAttribute attribute, ResourceContext resourceContext, string path)
        {
            if (!attribute.Capabilities.HasFlag(AttrCapabilities.AllowView))
            {
                throw new InvalidQueryStringParameterException(_lastParameterName, "Retrieving the requested attribute is not allowed.",
                    $"Retrieving the attribute '{attribute.PublicName}' is not allowed.");
            }
        }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            if (disableQueryStringAttribute == null) throw new ArgumentNullException(nameof(disableQueryStringAttribute));

            return !disableQueryStringAttribute.ContainsParameter(StandardQueryStringParameters.Fields);
        }

        /// <inheritdoc/>
        public bool CanRead(string parameterName)
        {
            var isNested = parameterName.StartsWith("fields[", StringComparison.Ordinal) && parameterName.EndsWith("]", StringComparison.Ordinal);
            return parameterName == "fields" || isNested;
        }

        /// <inheritdoc/>
        public void Read(string parameterName, StringValues parameterValue)
        {
            _lastParameterName = parameterName;

            try
            {
                ResourceFieldChainExpression scope = GetScope(parameterName);
                SparseFieldSetExpression sparseFieldSet = GetSparseFieldSet(parameterValue, scope);

                var expressionInScope = new ExpressionInScope(scope, sparseFieldSet);
                _constraints.Add(expressionInScope);
            }
            catch (QueryParseException exception)
            {
                throw new InvalidQueryStringParameterException(parameterName, "The specified fieldset is invalid.",
                    exception.Message, exception);
            }
        }

        private ResourceFieldChainExpression GetScope(string parameterName)
        {
            var parameterScope = _scopeParser.Parse(parameterName, RequestResource);
            return parameterScope.Scope;
        }

        private SparseFieldSetExpression GetSparseFieldSet(string parameterValue, ResourceFieldChainExpression scope)
        {
            ResourceContext resourceContextInScope = GetResourceContextForScope(scope);
            return _sparseFieldSetParser.Parse(parameterValue, resourceContextInScope);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _constraints;
        }
    }
}
