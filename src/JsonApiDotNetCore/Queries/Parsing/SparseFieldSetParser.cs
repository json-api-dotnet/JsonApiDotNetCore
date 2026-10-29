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
    public SparseFieldSetExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Tokenize(source);

        SparseFieldSetExpression expression = ParseSparseFieldSet(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected virtual SparseFieldSetExpression ParseSparseFieldSet(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var treeBuilder = new SparseFieldTreeBuilder();
        bool isFirstChain = true;

        while (TokenStack.Count > 0)
        {
            if (isFirstChain)
            {
                isFirstChain = false;
            }
            else
            {
                EatSingleCharacterToken(TokenKind.Comma);
            }

            ResourceFieldChainExpression nextChain = ParseFieldChain(BuiltInPatterns.CompoundOrCompoundCollectionChainEndingInField, resourceType);
            treeBuilder.IncludeChain(nextChain.Fields);
        }

        return treeBuilder.ToExpression();
    }

    protected override void ValidateField(ResourceFieldAttribute field, int position)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (field.IsViewBlocked())
        {
            string kind = field is AttrAttribute ? "attribute" : "relationship";
            throw new QueryParseException($"Retrieving {kind} '{field}' is not allowed.", position);
        }
    }
}
