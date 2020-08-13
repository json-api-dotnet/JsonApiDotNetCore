using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public class SparseFieldSetParser : QueryParser
    {
        private readonly Action<AttrAttribute, ResourceContext, string> _validateSingleAttributeCallback;
        private ResourceContext _resourceContextInScope;

        public SparseFieldSetParser(IResourceContextProvider resourceContextProvider, Action<AttrAttribute, ResourceContext, string> validateSingleAttributeCallback = null)
            : base(resourceContextProvider)
        {
            _validateSingleAttributeCallback = validateSingleAttributeCallback;
        }

        public SparseFieldSetExpression Parse(string source, ResourceContext resourceContextInScope)
        {
            _resourceContextInScope = resourceContextInScope ?? throw new ArgumentNullException(nameof(resourceContextInScope));
            Tokenize(source);

            var expression = ParseSparseFieldSet();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected SparseFieldSetExpression ParseSparseFieldSet()
        {
            var attributes = new Dictionary<string, AttrAttribute>();

            ResourceFieldChainExpression nextChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, "Attribute name expected.");
            AttrAttribute nextAttribute = nextChain.Fields.Cast<AttrAttribute>().Single();
            attributes[nextAttribute.PublicName] = nextAttribute;

            while (TokenStack.Any())
            {
                EatSingleCharacterToken(TokenKind.Comma);

                nextChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, "Attribute name expected.");
                nextAttribute = nextChain.Fields.Cast<AttrAttribute>().Single();
                attributes[nextAttribute.PublicName] = nextAttribute;
            }

            return new SparseFieldSetExpression(attributes.Values);
        }

        protected override IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            var attribute = ChainResolver.GetAttribute(path, _resourceContextInScope, path);

            _validateSingleAttributeCallback?.Invoke(attribute, _resourceContextInScope, path);

            return new[] {attribute};
        }
    }
}
