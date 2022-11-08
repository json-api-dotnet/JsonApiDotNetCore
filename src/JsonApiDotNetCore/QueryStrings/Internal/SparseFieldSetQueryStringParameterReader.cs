using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.QueryStrings.Internal;

[PublicAPI]
public class SparseFieldSetQueryStringParameterReader : QueryStringParameterReader, ISparseFieldSetQueryStringParameterReader
{
    private readonly SparseFieldTypeParser _sparseFieldTypeParser;
    private readonly SparseFieldSetParser _sparseFieldSetParser;

    private readonly ImmutableDictionary<ResourceType, SparseFieldSetExpression>.Builder _sparseFieldTableBuilder =
        ImmutableDictionary.CreateBuilder<ResourceType, SparseFieldSetExpression>();

    private string? _lastParameterName;

    /// <inheritdoc />
    bool IQueryStringParameterReader.AllowEmptyValue => true;

    public SparseFieldSetQueryStringParameterReader(IJsonApiRequest request, IResourceGraph resourceGraph)
        : base(request, resourceGraph)
    {
        _sparseFieldTypeParser = new SparseFieldTypeParser(resourceGraph);
        _sparseFieldSetParser = new SparseFieldSetParser(ValidateSingleField);
    }

    protected void ValidateSingleField(ResourceFieldAttribute field, ResourceType resourceType, string path)
    {
        if (field.IsViewBlocked())
        {
            string kind = field is AttrAttribute ? "attribute" : "relationship";

            throw new InvalidQueryStringParameterException(_lastParameterName!, $"Retrieving the requested {kind} is not allowed.",
                $"Retrieving the {kind} '{field.PublicName}' is not allowed.");
        }
    }

    /// <inheritdoc />
    public virtual bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        ArgumentGuard.NotNull(disableQueryStringAttribute);

        return !IsAtomicOperationsRequest && !disableQueryStringAttribute.ContainsParameter(JsonApiQueryStringParameters.Fields);
    }

    /// <inheritdoc />
    public virtual bool CanRead(string parameterName)
    {
        ArgumentGuard.NotNullNorEmpty(parameterName);

        return parameterName.StartsWith("fields[", StringComparison.Ordinal) && parameterName.EndsWith("]", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public virtual void Read(string parameterName, StringValues parameterValue)
    {
        _lastParameterName = parameterName;

        try
        {
            ResourceType targetResourceType = GetSparseFieldType(parameterName);
            SparseFieldSetExpression sparseFieldSet = GetSparseFieldSet(parameterValue, targetResourceType);

            _sparseFieldTableBuilder[targetResourceType] = sparseFieldSet;
        }
        catch (QueryParseException exception)
        {
            throw new InvalidQueryStringParameterException(parameterName, "The specified fieldset is invalid.", exception.Message, exception);
        }
    }

    private ResourceType GetSparseFieldType(string parameterName)
    {
        return _sparseFieldTypeParser.Parse(parameterName);
    }

    private SparseFieldSetExpression GetSparseFieldSet(string parameterValue, ResourceType resourceType)
    {
        SparseFieldSetExpression? sparseFieldSet = _sparseFieldSetParser.Parse(parameterValue, resourceType);

        if (sparseFieldSet == null)
        {
            // We add ID on an incoming empty fieldset, so that callers can distinguish between no fieldset and an empty one.
            AttrAttribute idAttribute = resourceType.GetAttributeByPropertyName(nameof(Identifiable<object>.Id));
            return new SparseFieldSetExpression(ImmutableHashSet.Create<ResourceFieldAttribute>(idAttribute));
        }

        return sparseFieldSet;
    }

    /// <inheritdoc />
    public virtual IReadOnlyCollection<ExpressionInScope> GetConstraints()
    {
        return _sparseFieldTableBuilder.Any()
            ? new ExpressionInScope(null, new SparseFieldTableExpression(_sparseFieldTableBuilder.ToImmutable())).AsArray()
            : Array.Empty<ExpressionInScope>();
    }
}
