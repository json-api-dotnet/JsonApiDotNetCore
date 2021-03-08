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
    public class SortParser : QueryExpressionParser
    {
        private readonly Action<ResourceFieldAttribute, ResourceContext, string> _validateSingleFieldCallback;
        private ResourceContext _resourceContextInScope;

        public SortParser(IResourceContextProvider resourceContextProvider,
            Action<ResourceFieldAttribute, ResourceContext, string> validateSingleFieldCallback = null)
            : base(resourceContextProvider)
        {
            _validateSingleFieldCallback = validateSingleFieldCallback;
        }

        public SortExpression Parse(string source, ResourceContext resourceContextInScope)
        {
            ArgumentGuard.NotNull(resourceContextInScope, nameof(resourceContextInScope));

            _resourceContextInScope = resourceContextInScope;

            Tokenize(source);

            SortExpression expression = ParseSort();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected SortExpression ParseSort()
        {
            SortElementExpression firstElement = ParseSortElement();

            List<SortElementExpression> elements = firstElement.AsList();

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

            string errorMessage = isAscending ? "-, count function or field name expected." : "Count function or field name expected.";
            ResourceFieldChainExpression targetAttribute = ParseFieldChain(FieldChainRequirements.EndsInAttribute, errorMessage);
            return new SortElementExpression(targetAttribute, isAscending);
        }

        protected override IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            if (chainRequirements == FieldChainRequirements.EndsInToMany)
            {
                return ChainResolver.ResolveToOneChainEndingInToMany(_resourceContextInScope, path);
            }

            if (chainRequirements == FieldChainRequirements.EndsInAttribute)
            {
                return ChainResolver.ResolveToOneChainEndingInAttribute(_resourceContextInScope, path, _validateSingleFieldCallback);
            }

            throw new InvalidOperationException($"Unexpected combination of chain requirement flags '{chainRequirements}'.");
        }
    }
}
