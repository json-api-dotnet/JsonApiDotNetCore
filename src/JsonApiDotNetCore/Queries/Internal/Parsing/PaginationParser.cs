using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    [PublicAPI]
    public class PaginationParser : QueryExpressionParser
    {
        private readonly Action<ResourceFieldAttribute, ResourceContext, string> _validateSingleFieldCallback;
        private ResourceContext _resourceContextInScope;

        public PaginationParser(IResourceContextProvider resourceContextProvider,
            Action<ResourceFieldAttribute, ResourceContext, string> validateSingleFieldCallback = null)
            : base(resourceContextProvider)
        {
            _validateSingleFieldCallback = validateSingleFieldCallback;
        }

        public PaginationQueryStringValueExpression Parse(string source, ResourceContext resourceContextInScope)
        {
            ArgumentGuard.NotNull(resourceContextInScope, nameof(resourceContextInScope));

            _resourceContextInScope = resourceContextInScope;

            Tokenize(source);

            PaginationQueryStringValueExpression expression = ParsePagination();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected PaginationQueryStringValueExpression ParsePagination()
        {
            var elements = new List<PaginationElementQueryStringValueExpression>();

            PaginationElementQueryStringValueExpression element = ParsePaginationElement();
            elements.Add(element);

            while (TokenStack.Any())
            {
                EatSingleCharacterToken(TokenKind.Comma);

                element = ParsePaginationElement();
                elements.Add(element);
            }

            return new PaginationQueryStringValueExpression(elements);
        }

        protected PaginationElementQueryStringValueExpression ParsePaginationElement()
        {
            int? number = TryParseNumber();

            if (number != null)
            {
                return new PaginationElementQueryStringValueExpression(null, number.Value);
            }

            ResourceFieldChainExpression scope = ParseFieldChain(FieldChainRequirements.EndsInToMany, "Number or relationship name expected.");

            EatSingleCharacterToken(TokenKind.Colon);

            number = TryParseNumber();

            if (number == null)
            {
                throw new QueryParseException("Number expected.");
            }

            return new PaginationElementQueryStringValueExpression(scope, number.Value);
        }

        protected int? TryParseNumber()
        {
            if (TokenStack.TryPeek(out Token nextToken))
            {
                int number;

                if (nextToken.Kind == TokenKind.Minus)
                {
                    TokenStack.Pop();

                    if (TokenStack.TryPop(out Token token) && token.Kind == TokenKind.Text && int.TryParse(token.Value, out number))
                    {
                        return -number;
                    }

                    throw new QueryParseException("Digits expected.");
                }

                if (nextToken.Kind == TokenKind.Text && int.TryParse(nextToken.Value, out number))
                {
                    TokenStack.Pop();
                    return number;
                }
            }

            return null;
        }

        protected override IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            return ChainResolver.ResolveToManyChain(_resourceContextInScope, path, _validateSingleFieldCallback);
        }
    }
}
