using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    /// <summary>
    /// The base class for parsing query string parameters, using the Recursive Descent algorithm.
    /// </summary>
    /// <remarks>
    /// Uses a tokenizer to populate a stack of tokens, which is then manipulated from the various parsing routines for subexpressions. Implementations
    /// should throw <see cref="QueryParseException" /> on invalid input.
    /// </remarks>
    [PublicAPI]
    public abstract class QueryExpressionParser
    {
        protected Stack<Token> TokenStack { get; private set; }
        private protected ResourceFieldChainResolver ChainResolver { get; }

        protected QueryExpressionParser(IResourceContextProvider resourceContextProvider)
        {
            ChainResolver = new ResourceFieldChainResolver(resourceContextProvider);
        }

        /// <summary>
        /// Takes a dotted path and walks the resource graph to produce a chain of fields.
        /// </summary>
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
                IReadOnlyCollection<ResourceFieldAttribute> chain = OnResolveFieldChain(token.Value, chainRequirements);

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
