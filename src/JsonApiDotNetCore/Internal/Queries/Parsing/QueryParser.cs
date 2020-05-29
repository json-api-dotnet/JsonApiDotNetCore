using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Queries.Expressions;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public abstract class QueryParser
    {
        private readonly ResolveFieldChainCallback _resolveFieldChainCallback;

        protected Stack<Token> TokenStack { get; }

        protected QueryParser(string source, ResolveFieldChainCallback resolveFieldChainCallback)
        {
            _resolveFieldChainCallback = resolveFieldChainCallback ?? throw new ArgumentNullException(nameof(resolveFieldChainCallback));

            var tokenizer = new QueryTokenizer(source);
            TokenStack = new Stack<Token>(tokenizer.EnumerateTokens().Reverse());
        }

        protected ResourceFieldChainExpression ParseFieldChain(FieldChainRequirements chainRequirements, string alternativeErrorMessage)
        {
            if (TokenStack.TryPop(out Token token) && token.Kind == TokenKind.Text)
            {
                var chain = _resolveFieldChainCallback(token.Value, chainRequirements);
                if (chain.Any())
                {
                    return new ResourceFieldChainExpression(chain);
                }
            }

            throw new QueryParseException(alternativeErrorMessage ?? "Field name expected.");
        }

        protected CountExpression TryParseCount()
        {
            if (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Text && nextToken.Value == Keywords.Count)
            {
                TokenStack.Pop();

                EatSingleCharacterToken(TokenKind.OpenParen);

                ResourceFieldChainExpression targetCollection = ParseFieldChain(FieldChainRequirements.EndsInToMany, null);

                EatSingleCharacterToken(TokenKind.CloseParen);

                return new CountExpression(targetCollection);
            }

            return null;
        }

        protected void EatText(string text)
        {
            if (!TokenStack.TryPop(out Token token) || token.Kind != TokenKind.Text || token.Value != text)
            {
                throw new QueryParseException(text + " expected.");
            }
        }

        protected void EatSingleCharacterToken(TokenKind kind)
        {
            if (!TokenStack.TryPop(out Token token) || token.Kind != kind)
            {
                char ch = QueryTokenizer.SingleCharacterToTokenKinds.Single(pair => pair.Value == kind).Key;
                throw new QueryParseException(ch + " expected.");
            }
        }

        protected void AssertTokenStackIsEmpty()
        {
            if (TokenStack.Any())
            {
                throw new QueryParseException("End of expression expected.");
            }
        }
    }
}
