using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Internal.QueryStrings;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public abstract class QueryParser
    {
        private protected ResourceFieldChainResolver ChainResolver { get; }

        protected Stack<Token> TokenStack { get; private set; }

        protected QueryParser(IResourceContextProvider resourceContextProvider)
        {
            ChainResolver = new ResourceFieldChainResolver(resourceContextProvider);
        }

        protected abstract IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements);

        protected virtual void Tokenize(string source)
        {
            var tokenizer = new QueryTokenizer(source);
            TokenStack = new Stack<Token>(tokenizer.EnumerateTokens().Reverse());
        }

        protected ResourceFieldChainExpression ParseFieldChain(FieldChainRequirements chainRequirements, string alternativeErrorMessage)
        {
            if (TokenStack.TryPop(out Token token) && token.Kind == TokenKind.Text)
            {
                var chain = OnResolveFieldChain(token.Value, chainRequirements);
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
