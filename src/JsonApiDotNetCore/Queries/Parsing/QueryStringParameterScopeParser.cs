using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="IQueryStringParameterScopeParser" />
[PublicAPI]
public class QueryStringParameterScopeParser : QueryExpressionParser, IQueryStringParameterScopeParser
{
    /// <inheritdoc />
    public QueryStringParameterScopeExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Tokenize(source);

        QueryStringParameterScopeExpression expression = ParseQueryStringParameterScope(resourceType);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected virtual QueryStringParameterScopeExpression ParseQueryStringParameterScope(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        int position = GetNextTokenPositionOrEnd();

        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text)
        {
            throw new QueryParseException("Parameter name expected.", position);
        }

        var name = new LiteralConstantExpression(token.Value!);

        IncludeExpression? scope = null;

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.OpenBracket)
        {
            TokenStack.Pop();

            scope = ParseRelationshipChainEndingInToMany(resourceType, null);

            EatSingleCharacterToken(TokenKind.CloseBracket);
        }

        return new QueryStringParameterScopeExpression(name, scope);
    }
}
