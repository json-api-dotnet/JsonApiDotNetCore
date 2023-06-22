using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public class PaginationParser : QueryExpressionParser
{
    private ResourceType? _resourceTypeInScope;

    public PaginationQueryStringValueExpression Parse(string source, ResourceType resourceTypeInScope)
    {
        ArgumentGuard.NotNull(resourceTypeInScope);

        _resourceTypeInScope = resourceTypeInScope;

        Tokenize(source);

        PaginationQueryStringValueExpression expression = ParsePagination();

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected PaginationQueryStringValueExpression ParsePagination()
    {
        ImmutableArray<PaginationElementQueryStringValueExpression>.Builder elementsBuilder =
            ImmutableArray.CreateBuilder<PaginationElementQueryStringValueExpression>();

        PaginationElementQueryStringValueExpression element = ParsePaginationElement();
        elementsBuilder.Add(element);

        while (TokenStack.Any())
        {
            EatSingleCharacterToken(TokenKind.Comma);

            element = ParsePaginationElement();
            elementsBuilder.Add(element);
        }

        return new PaginationQueryStringValueExpression(elementsBuilder.ToImmutable());
    }

    protected PaginationElementQueryStringValueExpression ParsePaginationElement()
    {
        int position = GetNextTokenPositionOrEnd();
        int? number = TryParseNumber();

        if (number != null)
        {
            return new PaginationElementQueryStringValueExpression(null, number.Value, position);
        }

        ResourceFieldChainExpression scope = ParseFieldChain(FieldChainRequirements.EndsInToMany, "Number or relationship name expected.");

        EatSingleCharacterToken(TokenKind.Colon);

        position = GetNextTokenPositionOrEnd();
        number = TryParseNumber();

        if (number == null)
        {
            throw new QueryParseException("Number expected.", position);
        }

        return new PaginationElementQueryStringValueExpression(scope, number.Value, position);
    }

    protected int? TryParseNumber()
    {
        if (TokenStack.TryPeek(out Token? nextToken))
        {
            int number;

            if (nextToken.Kind == TokenKind.Minus)
            {
                TokenStack.Pop();
                int position = GetNextTokenPositionOrEnd();

                if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text && int.TryParse(token.Value, out number))
                {
                    return -number;
                }

                throw new QueryParseException("Digits expected.", position);
            }

            if (nextToken.Kind == TokenKind.Text && int.TryParse(nextToken.Value, out number))
            {
                TokenStack.Pop();
                return number;
            }
        }

        return null;
    }

    protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, int position, FieldChainRequirements chainRequirements)
    {
        return ChainResolver.ResolveToManyChain(_resourceTypeInScope!, path, position);
    }
}
