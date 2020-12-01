using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    public class IncludeQueryStringParameterReader : QueryStringParameterReader, IIncludeQueryStringParameterReader
    {
        private readonly IJsonApiOptions _options;
        private readonly IncludeParser _includeParser;

        private IncludeExpression _includeExpression;
        private string _lastParameterName;

        public IncludeQueryStringParameterReader(IJsonApiRequest request, IResourceContextProvider resourceContextProvider, IJsonApiOptions options)
            : base(request, resourceContextProvider)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _includeParser = new IncludeParser(resourceContextProvider, ValidateSingleRelationship);
        }

        protected void ValidateSingleRelationship(RelationshipAttribute relationship, ResourceContext resourceContext, string path)
        {
            if (!relationship.CanInclude)
            {
                throw new InvalidQueryStringParameterException(_lastParameterName,
                    "Including the requested relationship is not allowed.",
                    path == relationship.PublicName
                        ? $"Including the relationship '{relationship.PublicName}' on '{resourceContext.PublicName}' is not allowed."
                        : $"Including the relationship '{relationship.PublicName}' in '{path}' on '{resourceContext.PublicName}' is not allowed.");
            }
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            if (disableQueryStringAttribute == null) throw new ArgumentNullException(nameof(disableQueryStringAttribute));

            return !IsAtomicOperationsRequest &&
                !disableQueryStringAttribute.ContainsParameter(StandardQueryStringParameters.Include);
        }

        /// <inheritdoc />
        public virtual bool CanRead(string parameterName)
        {
            return parameterName == "include";
        }

        /// <inheritdoc />
        public virtual void Read(string parameterName, StringValues parameterValue)
        {
            _lastParameterName = parameterName;

            try
            {
                _includeExpression = GetInclude(parameterValue);
            }
            catch (QueryParseException exception)
            {
                throw new InvalidQueryStringParameterException(parameterName, "The specified include is invalid.",
                    exception.Message, exception);
            }
        }

        private IncludeExpression GetInclude(string parameterValue)
        {
            return _includeParser.Parse(parameterValue, RequestResource, _options.MaximumIncludeDepth);
        }

        /// <inheritdoc />
        public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            var expressionInScope = _includeExpression != null
                ? new ExpressionInScope(null, _includeExpression)
                : new ExpressionInScope(null, IncludeExpression.Empty);

            return new[] {expressionInScope};
        }
    }
}
