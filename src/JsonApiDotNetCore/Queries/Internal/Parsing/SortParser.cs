using System;
using System.Collections.Immutable;
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
        private readonly Action<ResourceFieldAttribute, ResourceType, string> _validateSingleFieldCallback;
        private ResourceType _resourceTypeInScope;

        public SortParser(Action<ResourceFieldAttribute, ResourceType, string> validateSingleFieldCallback = null)
        {
            _validateSingleFieldCallback = validateSingleFieldCallback;
        }

        public SortExpression Parse(string source, ResourceType resourceTypeInScope)
        {
            ArgumentGuard.NotNull(resourceTypeInScope, nameof(resourceTypeInScope));

            _resourceTypeInScope = resourceTypeInScope;

            Tokenize(source);

            SortExpression expression = ParseSort();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected SortExpression ParseSort()
        {
            SortElementExpression firstElement = ParseSortElement();

            ImmutableArray<SortElementExpression>.Builder elementsBuilder = ImmutableArray.CreateBuilder<SortElementExpression>();
            elementsBuilder.Add(firstElement);

            while (TokenStack.Any())
            {
                EatSingleCharacterToken(TokenKind.Comma);

                SortElementExpression nextElement = ParseSortElement();
                elementsBuilder.Add(nextElement);
            }

            return new SortExpression(elementsBuilder.ToImmutable());
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

        protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            if (chainRequirements == FieldChainRequirements.EndsInToMany)
            {
                return ChainResolver.ResolveToOneChainEndingInToMany(_resourceTypeInScope, path);
            }

            if (chainRequirements == FieldChainRequirements.EndsInAttribute)
            {
                return ChainResolver.ResolveToOneChainEndingInAttribute(_resourceTypeInScope, path, _validateSingleFieldCallback);
            }

            throw new InvalidOperationException($"Unexpected combination of chain requirement flags '{chainRequirements}'.");
        }
    }
}
