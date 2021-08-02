using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    [PublicAPI]
    public class SparseFieldSetParser : QueryExpressionParser
    {
        private readonly Action<ResourceFieldAttribute, ResourceContext, string> _validateSingleFieldCallback;
        private ResourceContext _resourceContext;

        public SparseFieldSetParser(IResourceContextProvider resourceContextProvider,
            Action<ResourceFieldAttribute, ResourceContext, string> validateSingleFieldCallback = null)
            : base(resourceContextProvider)
        {
            _validateSingleFieldCallback = validateSingleFieldCallback;
        }

        public SparseFieldSetExpression Parse(string source, ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            _resourceContext = resourceContext;

            Tokenize(source);

            SparseFieldSetExpression expression = ParseSparseFieldSet();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected SparseFieldSetExpression ParseSparseFieldSet()
        {
            ImmutableHashSet<ResourceFieldAttribute>.Builder fieldSetBuilder = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();

            while (TokenStack.Any())
            {
                if (fieldSetBuilder.Count > 0)
                {
                    EatSingleCharacterToken(TokenKind.Comma);
                }

                ResourceFieldChainExpression nextChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, "Field name expected.");
                ResourceFieldAttribute nextField = nextChain.Fields.Single();
                fieldSetBuilder.Add(nextField);
            }

            return fieldSetBuilder.Any() ? new SparseFieldSetExpression(fieldSetBuilder.ToImmutable()) : null;
        }

        protected override IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            ResourceFieldAttribute field = ChainResolver.GetField(path, _resourceContext, path);

            _validateSingleFieldCallback?.Invoke(field, _resourceContext, path);

            return field.AsArray();
        }
    }
}
