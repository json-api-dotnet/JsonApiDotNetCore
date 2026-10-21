using System.Diagnostics;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.StringLength;

internal sealed class LengthFilterParser(IResourceFactory resourceFactory)
    : FilterParser(resourceFactory)
{
    protected override bool IsFunction(string name)
    {
        if (name == LengthExpression.Keyword)
        {
            return true;
        }

        return base.IsFunction(name);
    }

    protected override FunctionExpression ParseFunction()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: LengthExpression.Keyword })
        {
            return ParseLength();
        }

        return base.ParseFunction();
    }

    private LengthExpression ParseLength()
    {
        EatText(LengthExpression.Keyword);
        EatSingleCharacterToken(TokenKind.OpenParen);

        QueryExpression target = CurrentScope switch
        {
            ScalarCollectionElementFilterScope collectionScope => ParseScalarCollectionElement(collectionScope),
            AttributeContainerFilterScope containerScope => ParseTargetAttribute(containerScope),
            _ => throw new UnreachableException()
        };

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new LengthExpression(target);
    }

    private ScalarCollectionElementValueExpression ParseScalarCollectionElement(ScalarCollectionElementFilterScope collectionScope)
    {
        int position = GetNextTokenPositionOrEnd();
        ScalarCollectionElementValueExpression? elementValue = TryParseScalarCollectionElement(collectionScope);

        if (elementValue == null)
        {
            throw new QueryParseException("_ expected.", position);
        }

        if (collectionScope.ElementType != typeof(string))
        {
            throw new QueryParseException("Element of type 'String' expected.", position);
        }

        return elementValue;
    }

    private ResourceFieldChainExpression ParseTargetAttribute(AttributeContainerFilterScope containerScope)
    {
        int chainStartPosition = GetNextTokenPositionOrEnd();

        ResourceFieldChainExpression targetAttributeChain = ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInScalarAttribute,
            FieldChainPatternMatchOptions.None, containerScope.Container, null);

        ResourceFieldAttribute attribute = targetAttributeChain.Fields[^1];

        if (attribute.Property.PropertyType != typeof(string))
        {
            int position = chainStartPosition + GetRelativePositionOfLastFieldInChain(targetAttributeChain);
            throw new QueryParseException("Attribute of type 'String' expected.", position);
        }

        return targetAttributeChain;
    }
}
