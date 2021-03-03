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
            var fields = new Dictionary<string, ResourceFieldAttribute>();

            ResourceFieldChainExpression nextChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, "Field name expected.");
            ResourceFieldAttribute nextField = nextChain.Fields.Single();
            fields[nextField.PublicName] = nextField;

            while (TokenStack.Any())
            {
                EatSingleCharacterToken(TokenKind.Comma);

                nextChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, "Field name expected.");
                nextField = nextChain.Fields.Single();
                fields[nextField.PublicName] = nextField;
            }

            return new SparseFieldSetExpression(fields.Values);
        }

        protected override IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            ResourceFieldAttribute field = ChainResolver.GetField(path, _resourceContext, path);

            _validateSingleFieldCallback?.Invoke(field, _resourceContext, path);

            return field.AsArray();
        }
    }
}
