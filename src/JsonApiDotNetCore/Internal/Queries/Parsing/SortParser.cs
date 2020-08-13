using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public class SortParser : QueryParser
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
            _resourceContextInScope = resourceContextInScope ?? throw new ArgumentNullException(nameof(resourceContextInScope));
            Tokenize(source);

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
