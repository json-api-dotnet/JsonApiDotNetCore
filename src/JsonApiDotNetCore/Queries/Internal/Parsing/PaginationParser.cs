using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// Parses the JSON:API 'page' query string parameter value.
/// </summary>
[PublicAPI]
public class PaginationParser : QueryExpressionParser
{
    public PaginationQueryStringValueExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        Tokenize(source);

        PaginationQueryStringValueExpression expression = ParsePagination(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected PaginationQueryStringValueExpression ParsePagination(ResourceType resourceType)
    {
        ImmutableArray<PaginationElementQueryStringValueExpression>.Builder elementsBuilder =
            ImmutableArray.CreateBuilder<PaginationElementQueryStringValueExpression>();

        PaginationElementQueryStringValueExpression element = ParsePaginationElement(resourceType);
        elementsBuilder.Add(element);

        while (TokenStack.Any())
        {
            EatSingleCharacterToken(TokenKind.Comma);

            element = ParsePaginationElement(resourceType);
            elementsBuilder.Add(element);
        }

        return new PaginationQueryStringValueExpression(elementsBuilder.ToImmutable());
    }

    protected PaginationElementQueryStringValueExpression ParsePaginationElement(ResourceType resourceType)
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
}
