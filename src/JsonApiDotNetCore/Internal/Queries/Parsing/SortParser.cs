using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Queries.Expressions;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public class SortParser : QueryParser
    {
        public SortParser(string source, ResolveFieldChainCallback resolveFieldChainCallback)
            : base(source, resolveFieldChainCallback)
        {
        }

        public SortExpression Parse()
        {
            SortExpression expression = ParseSort();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected SortExpression ParseSort()
        {
            SortElementExpression firstElement = ParseSortElement();

            var elements = new List<SortElementExpression>
            {
                firstElement
            };

            while (TokenStack.Any())
            {
                EatSingleCharacterToken(TokenKind.Comma);

                SortElementExpression nextElement = ParseSortElement();
                elements.Add(nextElement);
            }

            return new SortExpression(elements);
        }

        protected SortElementExpression ParseSortElement()
        {
            bool isAscending = true;

            if (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Minus)
            {
                TokenStack.Pop();
                isAscending = false;
            }

            CountExpression count = TryParseCount();
            if (count != null)
            {
                return new SortElementExpression(count, isAscending);
            }

            var errorMessage = isAscending ? "-, count function or field name expected." : "Count function or field name expected.";
            ResourceFieldChainExpression targetAttribute = ParseFieldChain(FieldChainRequirements.EndsInAttribute, errorMessage);
            return new SortElementExpression(targetAttribute, isAscending);
        }
    }
}
