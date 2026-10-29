using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="ISortParser" />
[PublicAPI]
public class SortParser : QueryExpressionParser, ISortParser
{
    /// <inheritdoc />
    public SortExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Tokenize(source);

        SortExpression expression = ParseSort(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected virtual SortExpression ParseSort(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        SortElementExpression firstElement = ParseSortElement(resourceType);

        ImmutableArray<SortElementExpression>.Builder elementsBuilder = ImmutableArray.CreateBuilder<SortElementExpression>();
        elementsBuilder.Add(firstElement);

        while (TokenStack.Count > 0)
        {
            EatSingleCharacterToken(TokenKind.Comma);

            SortElementExpression nextElement = ParseSortElement(resourceType);
            elementsBuilder.Add(nextElement);
        }

        return new SortExpression(elementsBuilder.ToImmutable());
    }

    protected virtual SortElementExpression ParseSortElement(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        QueryParseChoices choices = QueryParseChoices.Minus | QueryParseChoices.CountFunction | QueryParseChoices.FieldName;

        if (TryEatSingleCharacterToken(TokenKind.Minus))
        {
            choices &= ~QueryParseChoices.Minus;
        }

        // An attribute or relationship name usually matches a single field, even when overridden in derived types.
        // But in the following case, two attributes are matched on GET /shoppingBaskets?sort=bonusPoints:
        //
        // public abstract class ShoppingBasket : Identifiable<long>
        // {
        // }
        //
        // public sealed class SilverShoppingBasket : ShoppingBasket
        // {
        //     [Attr]
        //     public short BonusPoints { get; set; }
        // }
        //
        // public sealed class PlatinumShoppingBasket : ShoppingBasket
        // {
        //     [Attr]
        //     public long BonusPoints { get; set; }
        // }
        //
        // In this case there are two distinct BonusPoints fields (with different data types). And the sort order depends
        // on which attribute is used.
        //
        // Because there is no syntax to pick one, ParseFieldChain() fails with an error. We could add optional upcast syntax
        // (which would be required in this case) in the future to make it work, if desired.

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text })
        {
            bool isAscending = choices.HasFlag(QueryParseChoices.Minus);

            if (IsFunction(nextToken.Value!))
            {
                QueryExpression functionTarget = ParseFunction(resourceType);
                return new SortElementExpression(functionTarget, isAscending);
            }

            if (!IsNullConstantToken(nextToken))
            {
                QueryExpression fieldChainTarget = ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInScalarAttribute, resourceType,
                    FieldChainPatternMatchOptions.AllowDerivedTypes);

                return new SortElementExpression(fieldChainTarget, isAscending);
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(choices, position);
    }

    protected virtual bool IsFunction(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return name == Keywords.Count;
    }

    protected virtual FunctionExpression ParseFunction(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Text)
        {
            switch (nextToken.Value)
            {
                case Keywords.Count:
                {
                    return ParseCount(resourceType);
                }
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(QueryParseChoices.CountFunction, position);
    }

    protected virtual CountExpression ParseCount(ResourceType resourceType)
    {
        EatText(Keywords.Count);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetCollection = ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInCollection, resourceType,
            FieldChainPatternMatchOptions.AllowDerivedTypes);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new CountExpression(targetCollection);
    }

    protected override void ValidateField(ResourceFieldAttribute field, int position)
    {
        ArgumentNullException.ThrowIfNull(field);

        // Intermediate compound attributes are skipped during capability checks because AllowSort was stripped at startup
        // to prevent sorting by whole compound objects, while built-in sort pattern '[OC]*S' ensures only leaf scalar attributes are evaluated.
        if (field is AttrAttribute { IsCompound: false, IsCollection: false } attribute && !attribute.Capabilities.HasFlag(AttrCapabilities.AllowSort))
        {
            throw new QueryParseException($"Sorting on attribute '{attribute}' is not allowed.", position);
        }
    }
}
