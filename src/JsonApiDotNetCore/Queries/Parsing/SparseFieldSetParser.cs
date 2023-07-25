using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="ISparseFieldSetParser" />
[PublicAPI]
public class SparseFieldSetParser : QueryExpressionParser, ISparseFieldSetParser
{
    /// <inheritdoc />
    public SparseFieldSetExpression? Parse(string source, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        Tokenize(source);

        SparseFieldSetExpression? expression = ParseSparseFieldSet(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected virtual SparseFieldSetExpression? ParseSparseFieldSet(ResourceType resourceType)
    {
        ImmutableHashSet<ResourceFieldAttribute>.Builder fieldSetBuilder = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();

        while (TokenStack.Any())
        {
            if (fieldSetBuilder.Count > 0)
            {
                EatSingleCharacterToken(TokenKind.Comma);
            }

            ResourceFieldChainExpression nextChain =
                ParseFieldChain(BuiltInPatterns.SingleField, FieldChainPatternMatchOptions.None, resourceType, "Field name expected.");

            ResourceFieldAttribute nextField = nextChain.Fields.Single();
            fieldSetBuilder.Add(nextField);
        }

        return fieldSetBuilder.Any() ? new SparseFieldSetExpression(fieldSetBuilder.ToImmutable()) : null;
    }

    protected override void ValidateField(ResourceFieldAttribute field, int position)
    {
        if (field.IsViewBlocked())
        {
            string kind = field is AttrAttribute ? "attribute" : "relationship";
            throw new QueryParseException($"Retrieving the {kind} '{field.PublicName}' is not allowed.", position);
        }
    }
}
