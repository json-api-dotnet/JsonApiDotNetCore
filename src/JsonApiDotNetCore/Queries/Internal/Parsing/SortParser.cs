using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// Parses the JSON:API 'sort' query string parameter value.
/// </summary>
[PublicAPI]
public class SortParser : QueryExpressionParser
{
    public SortExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        Tokenize(source);

        SortExpression expression = ParseSort(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected SortExpression ParseSort(ResourceType resourceType)
    {
        SortElementExpression firstElement = ParseSortElement(resourceType);

        ImmutableArray<SortElementExpression>.Builder elementsBuilder = ImmutableArray.CreateBuilder<SortElementExpression>();
        elementsBuilder.Add(firstElement);

        while (TokenStack.Any())
        {
            EatSingleCharacterToken(TokenKind.Comma);

            SortElementExpression nextElement = ParseSortElement(resourceType);
            elementsBuilder.Add(nextElement);
        }

        return new SortExpression(elementsBuilder.ToImmutable());
    }

    protected SortElementExpression ParseSortElement(ResourceType resourceType)
    {
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
        // Because there is no syntax to pick one, we fail with an error. We could add optional upcast syntax
        // (which would be required in this case) in the future to make it work, if desired.

        CountExpression? count = TryParseCount(FieldChainPatternMatchOptions.AllowDerivedTypes, resourceType);

        if (count != null)
        {
            return new SortElementExpression(count, isAscending);
        }

        string errorMessage = isAscending ? "-, count function or field name expected." : "Count function or field name expected.";

        ResourceFieldChainExpression targetAttribute = ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute,
            FieldChainPatternMatchOptions.AllowDerivedTypes, resourceType, errorMessage);

        return new SortElementExpression(targetAttribute, isAscending);
    }

    protected override void ValidateField(ResourceFieldAttribute field, int position)
    {
        if (field is AttrAttribute attribute && !attribute.Capabilities.HasFlag(AttrCapabilities.AllowSort))
        {
            throw new QueryParseException($"Sorting on attribute '{attribute.PublicName}' is not allowed.", position);
        }
    }
}
