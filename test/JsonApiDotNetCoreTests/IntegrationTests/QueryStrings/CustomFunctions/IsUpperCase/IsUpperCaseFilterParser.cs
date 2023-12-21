using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.IsUpperCase;

internal sealed class IsUpperCaseFilterParser(IResourceFactory resourceFactory) : FilterParser(resourceFactory)
{
    protected override FilterExpression ParseFilter()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: IsUpperCaseExpression.Keyword })
        {
            return ParseIsUpperCase();
        }

        return base.ParseFilter();
    }

    private IsUpperCaseExpression ParseIsUpperCase()
    {
        EatText(IsUpperCaseExpression.Keyword);
        EatSingleCharacterToken(TokenKind.OpenParen);

        int chainStartPosition = GetNextTokenPositionOrEnd();

        ResourceFieldChainExpression targetAttributeChain =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, ResourceTypeInScope, null);

        ResourceFieldAttribute attribute = targetAttributeChain.Fields[^1];

        if (attribute.Property.PropertyType != typeof(string))
        {
            int position = chainStartPosition + GetRelativePositionOfLastFieldInChain(targetAttributeChain);
            throw new QueryParseException("Attribute of type 'String' expected.", position);
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new IsUpperCaseExpression(targetAttributeChain);
    }
}
