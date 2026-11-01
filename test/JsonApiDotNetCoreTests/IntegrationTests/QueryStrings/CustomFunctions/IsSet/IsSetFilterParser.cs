using System.Diagnostics;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsSet;

internal sealed class IsSetFilterParser(IResourceFactory resourceFactory)
    : FilterParser(resourceFactory)
{
    // The 'A' token matches attributes of every kind: scalar, compound, scalar collection and compound collection.
    private static readonly FieldChainPattern ToOneOrCompoundChainEndingInAnyAttribute = FieldChainPattern.Parse("[OC]*A");

    protected override FilterExpression ParseFilter()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: IsSetExpression.Keyword })
        {
            return ParseIsSet();
        }

        return base.ParseFilter();
    }

    private IsSetExpression ParseIsSet()
    {
        EatText(IsSetExpression.Keyword);
        EatSingleCharacterToken(TokenKind.OpenParen);

        if (CurrentScope is not AttributeContainerFilterScope containerScope)
        {
            throw new UnreachableException();
        }

        ResourceFieldChainExpression targetAttributeChain = ParseFieldChain(ToOneOrCompoundChainEndingInAnyAttribute, containerScope.Container);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new IsSetExpression(targetAttributeChain);
    }
}
