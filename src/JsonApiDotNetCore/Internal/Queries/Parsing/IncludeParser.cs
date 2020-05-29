using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Queries.Expressions;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public class IncludeParser : QueryParser
    {
        public IncludeParser(string source, ResolveFieldChainCallback resolveFieldChainCallback) 
            : base(source, resolveFieldChainCallback)
        {
        }

        public IncludeExpression Parse()
        {
            var expression = ParseInclude();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected IncludeExpression ParseInclude()
        {
            ResourceFieldChainExpression firstChain =
                ParseFieldChain(FieldChainRequirements.IsRelationship, "Relationship name expected.");

            var chains = new List<ResourceFieldChainExpression>
            {
                firstChain
            };

            while (TokenStack.Any())
            {
                EatSingleCharacterToken(TokenKind.Comma);

                var nextChain = ParseFieldChain(FieldChainRequirements.IsRelationship, "Relationship name expected.");
                chains.Add(nextChain);
            }

            return new IncludeExpression(chains);
        }
    }
}
