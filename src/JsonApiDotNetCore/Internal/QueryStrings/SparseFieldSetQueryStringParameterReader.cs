using System.Collections.Generic;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Internal.Queries.Parsing;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.RequestServices.Contracts;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    /// <summary>
    /// Reads the 'fields' query string parameter and produces a set of query constraints from it.
    /// </summary>
    public interface ISparseFieldSetQueryStringParameterReader : IQueryStringParameterReader, IQueryConstraintProvider
    {
    }

    public class SparseFieldSetQueryStringParameterReader : QueryStringParameterReader, ISparseFieldSetQueryStringParameterReader
    {
        private readonly List<ExpressionInScope> _constraints = new List<ExpressionInScope>();

        private string _lastParameterName;

        public SparseFieldSetQueryStringParameterReader(ICurrentRequest currentRequest, IResourceContextProvider resourceContextProvider)
            : base(currentRequest, resourceContextProvider)
        {
        }

        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Fields);
        }

        public bool CanRead(string parameterName)
        {
            var isNested = parameterName.StartsWith("fields[") && parameterName.EndsWith("]");
            return parameterName == "fields" || isNested;
        }

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
            var parser = new QueryStringParameterScopeParser(parameterName,
                (path, _) => ChainResolver.ResolveRelationshipChain(RequestResource, path));

            var parameterScope = parser.Parse(FieldChainRequirements.IsRelationship);
            return parameterScope.Scope;
        }

        private SparseFieldSetExpression GetSparseFieldSet(string parameterValue, ResourceFieldChainExpression scope)
        {
            ResourceContext resourceContextInScope = GetResourceContextForScope(scope);

            var parser = new SparseFieldSetParser(parameterValue, 
                (path, _) => ResolveSingleAttribute(path, resourceContextInScope));

            return parser.Parse();
        }

        protected IReadOnlyCollection<ResourceFieldAttribute> ResolveSingleAttribute(string path, ResourceContext resourceContext)
        {
            var attribute = ChainResolver.GetAttribute(path, resourceContext, path);

            ValidateAttribute(attribute);

            return new[] {attribute};
        }

        private void ValidateAttribute(AttrAttribute attribute)
        {
            if (!attribute.Capabilities.HasFlag(AttrCapabilities.AllowView))
            {
                throw new InvalidQueryStringParameterException(_lastParameterName, "Retrieving the requested attribute is not allowed.",
                    $"Retrieving the attribute '{attribute.PublicName}' is not allowed.");
            }
        }

        public IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _constraints.AsReadOnly();
        }
    }
}
