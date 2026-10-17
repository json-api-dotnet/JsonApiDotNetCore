using System.Collections.Immutable;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="IPaginationParser" />
[PublicAPI]
public class PaginationParser : QueryExpressionParser, IPaginationParser
{
    /// <inheritdoc />
    public PaginationQueryStringValueExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Tokenize(source);

        PaginationQueryStringValueExpression expression = ParsePagination(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected virtual PaginationQueryStringValueExpression ParsePagination(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

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
        ArgumentNullException.ThrowIfNull(resourceType);

        int position = GetNextTokenPositionOrEnd();
        int? number = TryParseNumber();

        if (number != null)
        {
            return new PaginationElementQueryStringValueExpression(null, number.Value, position);
        }

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Text && !IsNullConstantToken(nextToken))
        {
            IncludeExpression scope = ParseRelationshipChainEndingInToMany(resourceType);

            EatSingleCharacterToken(TokenKind.Colon);

            position = GetNextTokenPositionOrEnd();
            number = TryParseNumber();

            if (number == null)
            {
                throw new QueryParseException(QueryParseChoices.Number, position);
            }

            return new PaginationElementQueryStringValueExpression(scope, number.Value, position);
        }

        throw new QueryParseException(QueryParseChoices.Number | QueryParseChoices.ToManyRelationshipName, position);
    }

    private int? TryParseNumber()
    {
        bool isNegative = TryEatSingleCharacterToken(TokenKind.Minus);

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Text && int.TryParse(nextToken.Value, out int number))
        {
            TokenStack.Pop();
            return isNegative ? -number : number;
        }

        if (isNegative)
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException(QueryParseChoices.Number, position);
        }

        return null;
    }
}
