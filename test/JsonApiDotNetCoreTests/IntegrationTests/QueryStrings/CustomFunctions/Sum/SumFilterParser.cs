using System.Numerics;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Sum;

internal sealed class SumFilterParser(IResourceFactory resourceFactory)
    : FilterParser(resourceFactory)
{
    private static readonly FieldChainPattern SingleToManyRelationshipChain = FieldChainPattern.Parse("M");

    protected override bool IsFunction(string name)
    {
        if (name == SumExpression.Keyword)
        {
            return true;
        }

        return base.IsFunction(name);
    }

    protected override FunctionExpression ParseFunction()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: SumExpression.Keyword })
        {
            return ParseSum();
        }

        return base.ParseFunction();
    }

    private SumExpression ParseSum()
    {
        EatText(SumExpression.Keyword);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetToManyRelationshipChain = ParseFieldChain(SingleToManyRelationshipChain, FieldChainPatternMatchOptions.None,
            ContainerInScope, "To-many relationship expected.");

        EatSingleCharacterToken(TokenKind.Comma);

        QueryExpression selector = ParseSumSelectorInScope(targetToManyRelationshipChain);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new SumExpression(targetToManyRelationshipChain, selector);
    }

    private QueryExpression ParseSumSelectorInScope(ResourceFieldChainExpression targetChain)
    {
        // TODO: Allow collection attribute.
        var toManyRelationship = (HasManyAttribute)targetChain.Fields.Single();

        using IDisposable scope = InScopeOfContainer(toManyRelationship.RightType);
        return ParseSumSelector();
    }

    private QueryExpression ParseSumSelector()
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text } && IsFunction(nextToken.Value!))
        {
            FunctionExpression function = ParseFunction();

            if (!IsNumericType(function.ReturnType))
            {
                throw new QueryParseException("Function that returns a numeric type expected.", position);
            }

            return function;
        }

        ResourceFieldChainExpression fieldChain = ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None,
            ContainerInScope, null);

        var attrAttribute = (AttrAttribute)fieldChain.Fields[^1];

        if (!IsNumericType(attrAttribute.Property.PropertyType))
        {
            throw new QueryParseException("Attribute of a numeric type expected.", position);
        }

        return fieldChain;
    }

    private static bool IsNumericType(Type type)
    {
        Type innerType = Nullable.GetUnderlyingType(type) ?? type;
        return innerType.GetInterfaces().Any(@interface => @interface.Name == typeof(INumber<>).Name && @interface.Namespace == typeof(INumber<>).Namespace);
    }
}
