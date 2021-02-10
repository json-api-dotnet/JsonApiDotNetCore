using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SparseFieldSetQueryStringParameterReader : QueryStringParameterReader, ISparseFieldSetQueryStringParameterReader
    {
        private readonly SparseFieldTypeParser _sparseFieldTypeParser;
        private readonly SparseFieldSetParser _sparseFieldSetParser;
        private readonly Dictionary<ResourceContext, SparseFieldSetExpression> _sparseFieldTable = new Dictionary<ResourceContext, SparseFieldSetExpression>();
        private string _lastParameterName;

        public SparseFieldSetQueryStringParameterReader(IJsonApiRequest request, IResourceContextProvider resourceContextProvider)
            : base(request, resourceContextProvider)
        {
            _sparseFieldTypeParser = new SparseFieldTypeParser(resourceContextProvider);
            _sparseFieldSetParser = new SparseFieldSetParser(resourceContextProvider, ValidateSingleField);
        }

        protected void ValidateSingleField(ResourceFieldAttribute field, ResourceContext resourceContext, string path)
        {
            if (field is AttrAttribute attribute && !attribute.Capabilities.HasFlag(AttrCapabilities.AllowView))
            {
                throw new InvalidQueryStringParameterException(_lastParameterName, "Retrieving the requested attribute is not allowed.",
                    $"Retrieving the attribute '{attribute.PublicName}' is not allowed.");
            }
        }

        /// <inheritdoc />
        public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
        {
            if (disableQueryStringAttribute == null) throw new ArgumentNullException(nameof(disableQueryStringAttribute));

            return !IsAtomicOperationsRequest &&
                !disableQueryStringAttribute.ContainsParameter(StandardQueryStringParameters.Fields);
        }

        /// <inheritdoc />
        public virtual bool CanRead(string parameterName)
        {
            return parameterName.StartsWith("fields[", StringComparison.Ordinal) && parameterName.EndsWith("]", StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public virtual void Read(string parameterName, StringValues parameterValue)
        {
            _lastParameterName = parameterName;

            try
            {
                var targetResource = GetSparseFieldType(parameterName);
                var sparseFieldSet = GetSparseFieldSet(parameterValue, targetResource);

                _sparseFieldTable[targetResource] = sparseFieldSet;
            }
            catch (QueryParseException exception)
            {
                throw new InvalidQueryStringParameterException(parameterName, "The specified fieldset is invalid.",
                    exception.Message, exception);
            }
        }

        private ResourceContext GetSparseFieldType(string parameterName)
        {
            return _sparseFieldTypeParser.Parse(parameterName);
        }

        private SparseFieldSetExpression GetSparseFieldSet(string parameterValue, ResourceContext resourceContext)
        {
            return _sparseFieldSetParser.Parse(parameterValue, resourceContext);
        }

        /// <inheritdoc />
        public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
        {
            return _sparseFieldTable.Any()
                ? new[]
                {
                    new ExpressionInScope(null, new SparseFieldTableExpression(_sparseFieldTable))
                }
                : Array.Empty<ExpressionInScope>();
        }
    }
}
