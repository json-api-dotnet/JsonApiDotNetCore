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

        bool isAscending = true;

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Minus)
        {
            TokenStack.Pop();
            isAscending = false;
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

        QueryExpression target;

        if (TokenStack.TryPeek(out nextToken) && nextToken is { Kind: TokenKind.Text } && IsFunction(nextToken.Value!))
        {
            target = ParseFunction(resourceType);
        }
        else
        {
            string errorMessage = !isAscending ? "Count function or field name expected." : "-, count function or field name expected.";
            target = ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.AllowDerivedTypes, resourceType, errorMessage);
        }

        return new SortElementExpression(target, isAscending);
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
        throw new QueryParseException("Count function expected.", position);
    }

    private CountExpression ParseCount(ResourceType resourceType)
    {
        EatText(Keywords.Count);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetCollection =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInToMany, FieldChainPatternMatchOptions.AllowDerivedTypes, resourceType, null);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new CountExpression(targetCollection);
    }

    protected override void ValidateField(ResourceFieldAttribute field, int position)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (field is AttrAttribute attribute && !attribute.Capabilities.HasFlag(AttrCapabilities.AllowSort))
        {
            throw new QueryParseException($"Sorting on attribute '{attribute}' is not allowed.", position);
        }
    }
}
