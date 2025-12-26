using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

internal sealed class DecryptFilterParser(IResourceFactory resourceFactory)
    : FilterParser(resourceFactory)
{
    protected override bool IsFunction(string name)
    {
        if (name == DecryptExpression.Keyword)
        {
            return true;
        }

        return base.IsFunction(name);
    }

    protected override FunctionExpression ParseFunction()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: DecryptExpression.Keyword })
        {
            return ParseDecrypt();
        }

        return base.ParseFunction();
    }

    private DecryptExpression ParseDecrypt()
    {
        EatText(DecryptExpression.Keyword);
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

        return new DecryptExpression(targetAttributeChain);
    }
}
