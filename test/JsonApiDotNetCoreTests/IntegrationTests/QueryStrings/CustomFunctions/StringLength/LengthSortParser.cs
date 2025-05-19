using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

internal sealed class LengthSortParser : SortParser
{
    protected override bool IsFunction(string name)
    {
        if (name == LengthExpression.Keyword)
        {
            return true;
        }

        return base.IsFunction(name);
    }

    protected override FunctionExpression ParseFunction(ResourceType resourceType)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: LengthExpression.Keyword })
        {
            return ParseLength(resourceType);
        }

        return base.ParseFunction(resourceType);
    }

    private LengthExpression ParseLength(ResourceType resourceType)
    {
        EatText(LengthExpression.Keyword);
        EatSingleCharacterToken(TokenKind.OpenParen);

        int chainStartPosition = GetNextTokenPositionOrEnd();

        ResourceFieldChainExpression targetAttributeChain = ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute,
            FieldChainPatternMatchOptions.AllowDerivedTypes, resourceType, null);

        ResourceFieldAttribute attribute = targetAttributeChain.Fields[^1];

        if (attribute.Property.PropertyType != typeof(string))
        {
            int position = chainStartPosition + GetRelativePositionOfLastFieldInChain(targetAttributeChain);
            throw new QueryParseException("Attribute of type 'String' expected.", position);
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new LengthExpression(targetAttributeChain);
    }
}
