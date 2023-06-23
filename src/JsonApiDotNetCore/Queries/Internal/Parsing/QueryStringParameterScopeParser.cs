using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// Parses a JSON:API query string parameter name, containing a resource field chain that indicates the scope its query string parameter value applies
/// to.
/// </summary>
[PublicAPI]
public class QueryStringParameterScopeParser : QueryExpressionParser
{
    public QueryStringParameterScopeExpression Parse(string source, ResourceType resourceType, FieldChainPattern pattern, FieldChainPatternMatchOptions options)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNull(pattern);

        Tokenize(source);

        QueryStringParameterScopeExpression expression = ParseQueryStringParameterScope(resourceType, pattern, options);

        AssertTokenStackIsEmpty();

        return expression;
    }

    protected QueryStringParameterScopeExpression ParseQueryStringParameterScope(ResourceType resourceType, FieldChainPattern pattern,
        FieldChainPatternMatchOptions options)
    {
        int position = GetNextTokenPositionOrEnd();

        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text)
        {
            throw new QueryParseException("Parameter name expected.", position);
        }

        var name = new LiteralConstantExpression(token.Value!);

        ResourceFieldChainExpression? scope = null;

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.OpenBracket)
        {
            TokenStack.Pop();

            scope = ParseFieldChain(pattern, options, resourceType, null);

            EatSingleCharacterToken(TokenKind.CloseBracket);
        }

        return new QueryStringParameterScopeExpression(name, scope);
    }
}
