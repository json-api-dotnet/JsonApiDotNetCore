using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public class SparseFieldSetParser : QueryParser
    {
        public SparseFieldSetParser(string source, ResolveFieldChainCallback resolveFieldChainCallback)
            : base(source, resolveFieldChainCallback)
        {
        }

        public SparseFieldSetExpression Parse()
        {
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
    }
}
