using JsonApiDotNetCore.Internal.Queries.Expressions;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public class QueryStringParameterScopeParser : QueryParser
    {
        public QueryStringParameterScopeParser(string source, ResolveFieldChainCallback resolveFieldChainCallback)
            : base(source, resolveFieldChainCallback)
        {
        }

        public QueryStringParameterScopeExpression Parse(FieldChainRequirements chainRequirements)
        {
            var expression = ParseQueryStringParameterScope(chainRequirements);

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected QueryStringParameterScopeExpression ParseQueryStringParameterScope(FieldChainRequirements chainRequirements)
        {
            if (!TokenStack.TryPop(out Token token) || token.Kind != TokenKind.Text)
            {
                throw new QueryParseException("Parameter name expected.");
            }

            var name = new LiteralConstantExpression(token.Value);

            ResourceFieldChainExpression scope = null;

            if (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.OpenBracket)
            {
                TokenStack.Pop();

                scope = ParseFieldChain(chainRequirements, null);

                EatSingleCharacterToken(TokenKind.CloseBracket);
            }

            return new QueryStringParameterScopeExpression(name, scope);
        }
    }
}
