using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="IPaginationParser" />
[PublicAPI]
public class PaginationParser : QueryExpressionParser, IPaginationParser
{
    /// <inheritdoc />
    public PaginationQueryStringValueExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        Tokenize(source);

        PaginationQueryStringValueExpression expression = ParsePagination(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected virtual PaginationQueryStringValueExpression ParsePagination(ResourceType resourceType)
    {
        ImmutableArray<PaginationElementQueryStringValueExpression>.Builder elementsBuilder =
            ImmutableArray.CreateBuilder<PaginationElementQueryStringValueExpression>();

        PaginationElementQueryStringValueExpression element = ParsePaginationElement(resourceType);
        elementsBuilder.Add(element);

        while (TokenStack.Count > 0)
        {
            EatSingleCharacterToken(TokenKind.Comma);

            element = ParsePaginationElement(resourceType);
            elementsBuilder.Add(element);
        }

        return new PaginationQueryStringValueExpression(elementsBuilder.ToImmutable());
    }

    protected virtual PaginationElementQueryStringValueExpression ParsePaginationElement(ResourceType resourceType)
    {
        int position = GetNextTokenPositionOrEnd();
        int? number = TryParseNumber();

        if (number != null)
        {
            return new PaginationElementQueryStringValueExpression(null, number.Value, position);
        }

        ResourceFieldChainExpression scope = ParseFieldChain(BuiltInPatterns.RelationshipChainEndingInToMany, FieldChainPatternMatchOptions.None, resourceType,
            "Number or relationship name expected.");

        EatSingleCharacterToken(TokenKind.Colon);

        position = GetNextTokenPositionOrEnd();
        number = TryParseNumber();

        if (number == null)
        {
            throw new QueryParseException("Number expected.", position);
        }

        return new PaginationElementQueryStringValueExpression(scope, number.Value, position);
    }

    private int? TryParseNumber()
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
}
