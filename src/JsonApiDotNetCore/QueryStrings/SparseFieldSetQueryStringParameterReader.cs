using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings;

/// <inheritdoc cref="ISparseFieldSetQueryStringParameterReader" />
[PublicAPI]
public class SparseFieldSetQueryStringParameterReader : QueryStringParameterReader, ISparseFieldSetQueryStringParameterReader
{
    private readonly ISparseFieldTypeParser _scopeParser;
    private readonly ISparseFieldSetParser _sparseFieldSetParser;

    private readonly ImmutableDictionary<ResourceType, SparseFieldSetExpression>.Builder _sparseFieldTableBuilder =
        ImmutableDictionary.CreateBuilder<ResourceType, SparseFieldSetExpression>();

    /// <inheritdoc />
    public bool AllowEmptyValue => true;

    public SparseFieldSetQueryStringParameterReader(ISparseFieldTypeParser scopeParser, ISparseFieldSetParser sparseFieldSetParser, IJsonApiRequest request,
        IResourceGraph resourceGraph)
        : base(request, resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(scopeParser);
        ArgumentNullException.ThrowIfNull(sparseFieldSetParser);

        _scopeParser = scopeParser;
        _sparseFieldSetParser = sparseFieldSetParser;
    }

    /// <inheritdoc />
    public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        ArgumentNullException.ThrowIfNull(disableQueryStringAttribute);

        return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Fields);
    }

    /// <inheritdoc />
    public virtual bool CanRead(string parameterName)
    {
        ArgumentException.ThrowIfNullOrEmpty(parameterName);

        return parameterName.StartsWith("fields[", StringComparison.Ordinal) && parameterName.EndsWith(']');
    }

    /// <inheritdoc />
    public virtual void Read(string parameterName, StringValues parameterValue)
    {
        bool parameterNameIsValid = false;

        try
        {
            ResourceType resourceType = GetScope(parameterName);
            parameterNameIsValid = true;

            SparseFieldSetExpression sparseFieldSet = GetSparseFieldSet(parameterValue.ToString(), resourceType);
            _sparseFieldTableBuilder[resourceType] = sparseFieldSet;
        }
        catch (QueryParseException exception)
        {
            string specificMessage = exception.GetMessageWithPosition(parameterNameIsValid ? parameterValue.ToString() : parameterName);
            throw new InvalidQueryStringParameterException(parameterName, "The specified fieldset is invalid.", specificMessage, exception);
        }
    }

    private ResourceType GetScope(string parameterName)
    {
        return _scopeParser.Parse(parameterName);
    }

    private SparseFieldSetExpression GetSparseFieldSet(string parameterValue, ResourceType resourceType)
    {
        SparseFieldSetExpression? sparseFieldSet = _sparseFieldSetParser.Parse(parameterValue, resourceType);

        if (sparseFieldSet == null)
        {
            // We add ID to an incoming empty fieldset, so that callers can distinguish between no fieldset and an empty one.
            AttrAttribute idAttribute = resourceType.GetAttributeByPropertyName(nameof(Identifiable<>.Id));
            return new SparseFieldSetExpression(ImmutableHashSet.Create<ResourceFieldAttribute>(idAttribute));
        }

        return sparseFieldSet;
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        return _sparseFieldTableBuilder.Count > 0
            ? [new ExpressionInScope(null, new SparseFieldTableExpression(_sparseFieldTableBuilder.ToImmutable()))]
            : Array.Empty<ExpressionInScope>();
    }
}
